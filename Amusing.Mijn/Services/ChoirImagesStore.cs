using System.Data.Common;
using System.Security.Cryptography;
using MySqlConnector;
namespace Amusing.Mijn.Services;
public sealed record ChoirImageState(byte[]? Stored,string Version)
{
    public static string Stamp(byte[]? bytes)=>Convert.ToHexString(SHA256.HashData(bytes ?? [])).ToLowerInvariant();
}
public enum ImageSaveResult { Saved,Forbidden,Conflict }
public sealed class ChoirImagesStore(MySqlDataSource source, IHttpContextAccessor? supportContext = null)
{
    private static string Column(string kind)=>kind switch {"foto"=>"foto","logo"=>"logo",_=>throw new ArgumentException("Unknown image kind")};
    public async Task<bool> CanEditAsync(uint actor,uint group,CancellationToken ct)
    {
        if (SupportMode.CanSupport(supportContext,actor,group)) return true;
        await using var cmd=source.CreateCommand("SELECT EXISTS(SELECT 1 FROM ah_personen_rollen WHERE persoon_id=@actor AND zanggroep_id=@group AND rol IN ('contactpersoon1','contactpersoon2'))");
        Add(cmd,"@actor",actor);Add(cmd,"@group",group);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct))==1;
    }
    public async Task<ChoirImageState?> GetAsync(uint actor,uint group,string kind,CancellationToken ct)
    {
        await using var cmd=source.CreateCommand($"SELECT {Column(kind)} FROM ah_zanggroepen g WHERE zanggroep_id=@group AND (@support OR EXISTS(SELECT 1 FROM ah_personen_rollen r WHERE r.zanggroep_id=g.zanggroep_id AND r.persoon_id=@actor))");
        Add(cmd,"@actor",actor);Add(cmd,"@group",group);
        Add(cmd,"@support",SupportMode.CanSupport(supportContext,actor,group));
        var value=await cmd.ExecuteScalarAsync(ct);
        if(value is null) return null;
        var bytes=value is DBNull?null:(byte[])value;
        return new(bytes,ChoirImageState.Stamp(bytes));
    }
    /// <summary>Store canonical image bytes from ChoirImageProcessor, or null to remove.</summary>
    public async Task<ImageSaveResult> SaveAsync(uint actor,uint group,string kind,string expectedVersion,byte[]? replacement,CancellationToken ct)
    {
        var column=Column(kind);
        if(replacement is {Length:>12*1024*1024}) throw new InvalidDataException("Image too large");
        await using var connection=await source.OpenConnectionAsync(ct);
        await using var tx=await connection.BeginTransactionAsync(ct);
        if (!SupportMode.CanSupport(supportContext,actor,group))
        await using(var roles=connection.CreateCommand())
        {
            roles.Transaction=tx;roles.CommandText="SELECT rol FROM ah_personen_rollen WHERE persoon_id=@actor AND zanggroep_id=@group FOR UPDATE";
            Add(roles,"@actor",actor);Add(roles,"@group",group);
            await using var reader=await roles.ExecuteReaderAsync(ct);bool edit=false;
            while(await reader.ReadAsync(ct)) edit|=reader.GetString(0) is "contactpersoon1" or "contactpersoon2";
            if(!edit) return ImageSaveResult.Forbidden;
        }
        byte[]? old;
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText=$"SELECT {column} FROM ah_zanggroepen WHERE zanggroep_id=@group FOR UPDATE";Add(cmd,"@group",group);
            var value=await cmd.ExecuteScalarAsync(ct);if(value is null) return ImageSaveResult.Forbidden;
            old=value is DBNull?null:(byte[])value;
        }
        if(expectedVersion!=ChoirImageState.Stamp(old)) return ImageSaveResult.Conflict;
        if((old ?? []).AsSpan().SequenceEqual(replacement ?? [])) return ImageSaveResult.Saved;
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText=$"UPDATE ah_zanggroepen SET {column}=@bytes WHERE zanggroep_id=@group";
            Add(cmd,"@bytes",(object?)replacement ?? DBNull.Value);Add(cmd,"@group",group);await cmd.ExecuteNonQueryAsync(ct);
        }
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText="INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,group_id,changed_fields) VALUES(UTC_TIMESTAMP(6),@actorType,@actor,@action,@group,@field)";
            Add(cmd,"@actor",actor);Add(cmd,"@actorType",SupportMode.AuditType(supportContext,actor,group)); Add(cmd,"@action",replacement is null?"ChoirImageRemoved":"ChoirImageChanged");Add(cmd,"@group",group);Add(cmd,"@field",kind);await cmd.ExecuteNonQueryAsync(ct);
        }
        await tx.CommitAsync(ct);return ImageSaveResult.Saved;
    }
    private static void Add(DbCommand cmd,string name,object value){var p=cmd.CreateParameter();p.ParameterName=name;p.Value=value;cmd.Parameters.Add(p);}
}
