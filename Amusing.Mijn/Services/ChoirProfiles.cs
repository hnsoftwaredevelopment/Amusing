using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using MySqlConnector;
namespace Amusing.Mijn.Services;

public sealed class ChoirProfiles(MySqlDataSource source)
{
    private const string SelectProfile = """
        SELECT g.naam,g.standplaats,g.land,g.genre_id,g.website,COALESCE(d.email,''),g.beschrijving,
        CASE WHEN EXISTS(SELECT 1 FROM ah_personen_rollen r WHERE r.zanggroep_id=g.zanggroep_id AND r.persoon_id=@actor AND r.rol IN ('contactpersoon1','contactpersoon2','penningmeester')) THEN g.rekeningnr ELSE NULL END,
        EXISTS(SELECT 1 FROM ah_personen_rollen r WHERE r.zanggroep_id=g.zanggroep_id AND r.persoon_id=@actor AND r.rol IN ('contactpersoon1','contactpersoon2')),
        EXISTS(SELECT 1 FROM ah_personen_rollen r WHERE r.zanggroep_id=g.zanggroep_id AND r.persoon_id=@actor AND r.rol IN ('contactpersoon1','contactpersoon2','penningmeester'))
        FROM ah_zanggroepen g LEFT JOIN ah_zanggroep_details d ON d.id=g.zanggroep_id
        WHERE g.zanggroep_id=@group AND EXISTS(SELECT 1 FROM ah_personen_rollen r WHERE r.zanggroep_id=g.zanggroep_id AND r.persoon_id=@actor)
        """;
    public async Task<ChoirProfile?> GetAsync(uint actor,uint group,CancellationToken ct)
    {
        await using var connection=await source.OpenConnectionAsync(ct);
        return await LoadAsync(connection,null,actor,group,false,ct);
    }
    private static async Task<ChoirProfile?> LoadAsync(DbConnection connection,DbTransaction? tx,uint actor,uint group,bool locked,CancellationToken ct)
    {
        await using var cmd=connection.CreateCommand(); cmd.Transaction=tx;
        cmd.CommandText=SelectProfile+(locked ? " FOR UPDATE" : "");
        Add(cmd,"@actor",actor); Add(cmd,"@group",group);
        await using var reader=await cmd.ExecuteReaderAsync(ct);
        if(!await reader.ReadAsync(ct)) return null;
        string Text(int i)=>reader.IsDBNull(i) ? "" : reader.GetString(i);
        var input=new ChoirProfileInput {Name=Text(0),City=Text(1),Country=Text(2),GenreId=reader.IsDBNull(3)?0:Convert.ToInt32(reader.GetValue(3)),Website=Text(4),Email=Text(5),Description=Text(6),BankAccount=Text(7)};
        return new(input.Name,input.City,input.Country,input.GenreId,input.Website,input.Email ?? "",input.Description,reader.GetBoolean(9)?input.BankAccount:null,reader.GetBoolean(8),reader.GetBoolean(9),input.Stamp());
    }
    public async Task<List<ProfileOption>> OptionsAsync(bool countries,CancellationToken ct)
    {
        await using var cmd=source.CreateCommand(countries ? "SELECT code,naam FROM ah_landen ORDER BY naam" : "SELECT genre_id,nl FROM ah_genres WHERE genre_id BETWEEN 1 AND 255 ORDER BY nl");
        await using var reader=await cmd.ExecuteReaderAsync(ct);
        var options=new List<ProfileOption>();
        while(await reader.ReadAsync(ct)) options.Add(new(Convert.ToString(reader.GetValue(0),System.Globalization.CultureInfo.InvariantCulture)!,reader.GetString(1)));
        return options;
    }
    public async Task<ProfileSaveResult> SaveAsync(uint actor,uint group,ChoirProfileInput input,CancellationToken ct)
    {
        await using var connection=await source.OpenConnectionAsync(ct);
        await using var tx=await connection.BeginTransactionAsync(ct);
        // Lock the membership rows so a concurrent revocation cannot race this mutation.
        await using(var roles=connection.CreateCommand())
        {
            roles.Transaction=tx; roles.CommandText="SELECT rol FROM ah_personen_rollen WHERE persoon_id=@actor AND zanggroep_id=@group FOR UPDATE";
            Add(roles,"@actor",actor); Add(roles,"@group",group);
            await using var reader=await roles.ExecuteReaderAsync(ct);
            bool edit=false;
            while(await reader.ReadAsync(ct)) edit |= reader.GetString(0) is "contactpersoon1" or "contactpersoon2";
            if(!edit) return ProfileSaveResult.Forbidden;
        }
        var current=await LoadAsync(connection,tx,actor,group,true,ct);
        if(current is null) return ProfileSaveResult.Forbidden;
        input.Normalize();
        if(!Validator.TryValidateObject(input,new ValidationContext(input),new List<ValidationResult>(),true)) return ProfileSaveResult.Invalid;
        await using(var refs=connection.CreateCommand())
        {
            refs.Transaction=tx; refs.CommandText="SELECT EXISTS(SELECT 1 FROM ah_genres WHERE genre_id=@genre) AND EXISTS(SELECT 1 FROM ah_landen WHERE code=@country)";
            Add(refs,"@genre",input.GenreId); Add(refs,"@country",input.Country);
            if(Convert.ToInt32(await refs.ExecuteScalarAsync(ct))!=1) return ProfileSaveResult.Invalid;
        }
        if(input.Version!=current.Version) return ProfileSaveResult.Conflict;
        var old=ChoirProfileInput.From(current);
        var changes=new List<string>();
        if(input.Name!=old.Name) changes.Add(nameof(input.Name));
        if(input.City!=old.City) changes.Add(nameof(input.City));
        if(input.Country!=old.Country) changes.Add(nameof(input.Country));
        if(input.GenreId!=old.GenreId) changes.Add(nameof(input.GenreId));
        if(input.Website!=old.Website) changes.Add(nameof(input.Website));
        if((input.Email ?? "")!=(old.Email ?? "")) changes.Add(nameof(input.Email));
        if(input.Description!=old.Description) changes.Add(nameof(input.Description));
        if(input.BankAccount!=old.BankAccount) changes.Add(nameof(input.BankAccount));
        if(changes.Count==0) return ProfileSaveResult.Saved;
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;
            cmd.CommandText="UPDATE ah_zanggroepen SET naam=@name,standplaats=@city,land=@country,genre_id=@genre,website=@website,beschrijving=@description,rekeningnr=@bank WHERE zanggroep_id=@group";
            Add(cmd,"@name",input.Name); Add(cmd,"@city",input.City); Add(cmd,"@country",input.Country); Add(cmd,"@genre",input.GenreId); Add(cmd,"@website",input.Website); Add(cmd,"@description",input.Description); Add(cmd,"@bank",input.BankAccount); Add(cmd,"@group",group);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx; cmd.CommandText="INSERT INTO ah_zanggroep_details(id,email) VALUES(@group,@email) ON DUPLICATE KEY UPDATE email=@email";
            Add(cmd,"@group",group); Add(cmd,"@email",input.Email ?? ""); await cmd.ExecuteNonQueryAsync(ct);
        }
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;
            cmd.CommandText="INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,group_id,changed_fields) VALUES(UTC_TIMESTAMP(6),'choir_person',@actor,'ChoirProfileChanged',@group,@fields)";
            Add(cmd,"@actor",actor); Add(cmd,"@group",group); Add(cmd,"@fields",string.Join(',',changes)); await cmd.ExecuteNonQueryAsync(ct);
        }
        await tx.CommitAsync(ct);
        return ProfileSaveResult.Saved;
    }
    private static void Add(DbCommand cmd,string name,object value)
    {
        var p=cmd.CreateParameter();p.ParameterName=name;p.Value=value;cmd.Parameters.Add(p);
    }
}
