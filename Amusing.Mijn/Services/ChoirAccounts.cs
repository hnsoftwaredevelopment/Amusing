using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MySqlConnector;
namespace Amusing.Mijn.Services;

public sealed record ChoirAccount(uint Id, string Name, string Hash, string Email);
public sealed record ChoirSummary(uint Id, string Name, bool HasLogo = false);
public sealed class ChoirAccounts(MySqlDataSource source)
{
    public static string Stamp(string hash) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hash)));
    public async Task<ChoirAccount?> FindAsync(string email, CancellationToken ct)
    {
        await using var cmd = source.CreateCommand("SELECT p.persoon_id, p.voornaam, w.hash, p.email FROM ah_personen p JOIN ah_personen_wachtwoorden w ON w.id=p.persoon_id WHERE p.email=@email AND p.actief=1 LIMIT 2");
        Add(cmd, "@email", email);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        var account = new ChoirAccount(reader.GetFieldValue<uint>(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
        return await reader.ReadAsync(ct) ? null : account;
    }
    public async Task<bool> IsCurrentAsync(ClaimsPrincipal principal, CancellationToken ct)
    {
        if (principal.FindFirstValue("actor_type") != "choir_person" || !uint.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return false;
        await using var cmd = source.CreateCommand("SELECT w.hash FROM ah_personen p JOIN ah_personen_wachtwoorden w ON w.id=p.persoon_id WHERE p.persoon_id=@id AND p.actief=1");
        Add(cmd, "@id", id);
        return await cmd.ExecuteScalarAsync(ct) is string hash && Stamp(hash) == principal.FindFirstValue("credential_stamp");
    }
    public async Task<List<ChoirSummary>> GroupsAsync(uint personId, uint? groupId, CancellationToken ct)
    {
        await using var cmd = source.CreateCommand("SELECT DISTINCT g.zanggroep_id,g.naam,COALESCE(OCTET_LENGTH(g.logo)>0,0) FROM ah_zanggroepen g JOIN ah_personen_rollen r ON r.zanggroep_id=g.zanggroep_id WHERE r.persoon_id=@person AND (@group IS NULL OR g.zanggroep_id=@group) ORDER BY g.naam");
        Add(cmd, "@person", personId);
        Add(cmd, "@group", (object?)groupId ?? DBNull.Value);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var result = new List<ChoirSummary>();
        while (await reader.ReadAsync(ct)) result.Add(new(reader.GetFieldValue<uint>(0), reader.GetString(1), reader.GetBoolean(2)));
        return result;
    }

    public async Task<string> LanguageAsync(uint actor, CancellationToken ct)
    {
        await using var command = source.CreateCommand("SELECT taal FROM mijn_taalvoorkeur WHERE persoon_id=@actor");
        Add(command, "@actor", actor);
        return (await command.ExecuteScalarAsync(ct) as string) ?? "nl";
    }

    public async Task SaveLanguageAsync(uint actor, string language, CancellationToken ct)
    {
        await using var command = source.CreateCommand("""
            INSERT INTO mijn_taalvoorkeur(persoon_id,taal) VALUES(@actor,@language)
            ON DUPLICATE KEY UPDATE taal=VALUES(taal)
            """);
        Add(command, "@actor", actor);
        Add(command, "@language", language);
        await command.ExecuteNonQueryAsync(ct);
    }

    private static void Add(System.Data.Common.DbCommand cmd, string name, object value) { var p=cmd.CreateParameter(); p.ParameterName=name; p.Value=value; cmd.Parameters.Add(p); }
}
