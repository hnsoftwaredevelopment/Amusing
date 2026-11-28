using System.Security.Cryptography;
using System.Text;
using Amusing.Authentication;
using MySqlConnector;

namespace Amusing.Mijn.Services;

public sealed class AccountPasswords(MySqlDataSource source, IConfiguration configuration,
    ChoirAccounts accounts, ChoirPasswordVerifier verifier, ILogger<AccountPasswords> logger)
{
    public static bool ValidPassword(string password, string repeated) =>
        PasswordRecoveryRules.ValidPassword(password,repeated);

    public static bool ValidToken(string token) => PasswordRecoveryRules.ValidToken(token);

    public async Task RequestAsync(string email, CancellationToken ct, string? username = null, string language = "nl")
    {
        var beheer = username is not null;
        var origin = new Uri(configuration["AccountRegistration:Origin"]!, UriKind.Absolute);
        if (origin.Scheme != Uri.UriSchemeHttps) throw new InvalidOperationException("The reset origin must use HTTPS.");
        string token;
        if (beheer)
        {
            var request = await new Amusing.Authentication.MySql.BeheerPasswordRecoveryStore(source).IssueAsync(username!,email,ct);
            if (request is null) return;
            token=request.Token;email=request.Recipient;
        }
        else
        {
            var account=await accounts.FindAsync(email,ct);
            if(account is null)return;
            email=account.Email;
            token=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            await using var connection=await source.OpenConnectionAsync(ct);
            await using var command=new MySqlCommand("""
                INSERT INTO mijn_password_reset(token_hash,person_id,actor_type,credential_stamp,expires_at)
                VALUES(@token,@actor,'choir_person',@stamp,DATE_ADD(UTC_TIMESTAMP(6),INTERVAL 1 HOUR))
                """,connection);
            command.Parameters.AddWithValue("@token",HashToken(token));command.Parameters.AddWithValue("@actor",account.Id);
            command.Parameters.AddWithValue("@stamp",ChoirAccounts.Stamp(account.Hash+"|"+email));
            await command.ExecuteNonQueryAsync(ct);
        }

        try
        {
            var url = new Uri(origin, "/account/wachtwoord-instellen?token=" + token + (beheer ? "&beheer=1" : ""));
            var (subject, body) = language switch
            {
                "de" => ("Setze dein Mijn-Amusing-Passwort zurück",
                    $"Öffne {url}, um ein neues Passwort zu wählen. Der Link ist eine Stunde gültig. Falls du dies nicht angefordert hast, kannst du diese Nachricht ignorieren."),
                "en" => ("Reset your Mijn Amusing password",
                    $"Open {url} to choose a new password. The link is valid for one hour. If you did not request this, you can ignore this message."),
                _ => ("Stel je Mijn Amusing-wachtwoord opnieuw in",
                    $"Open {url} om een nieuw wachtwoord te kiezen. Deze link is één uur geldig. Heb je dit niet aangevraagd? Dan kun je dit bericht negeren.")
            };
            await MijnMail.DeliverAsync(configuration, email, subject, body, ct, anonymousAccountMail: true);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("Mail voor wachtwoordherstel kon niet worden bezorgd ({ErrorType}).", ex.GetType().Name);
        }
    }

    public async Task<bool> ResetAsync(string token, string password, CancellationToken ct, bool beheer = false)
    {
        if (beheer) return await new Amusing.Authentication.MySql.BeheerPasswordRecoveryStore(source).ResetAsync(token,password,ct);
        if (!ValidToken(token)) return false;

        await using var connection = await source.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        await using var lookup = new MySqlCommand("""
            SELECT person_id,credential_stamp FROM mijn_password_reset
            WHERE token_hash=@token AND actor_type=@type AND expires_at>UTC_TIMESTAMP(6)
            """, connection, tx);
        lookup.Parameters.AddWithValue("@token", HashToken(token));
        lookup.Parameters.AddWithValue("@type", "choir_person");
        uint actor;
        string stamp;
        await using (var reader = await lookup.ExecuteReaderAsync(ct))
        {
            if (!await reader.ReadAsync(ct)) return false;
            actor = reader.GetUInt32(0);
            stamp = reader.GetString(1);
        }

        var current = await LockPasswordAsync(connection, tx, actor, ct, includeEmail: true);
        if (current is null) return false;
        // Accept older reset links as long as the address change has not removed their token.
        var legacyStamp = ChoirAccounts.Stamp(current[..current.IndexOf('|')]) == stamp;
        if (ChoirAccounts.Stamp(current) != stamp && !legacyStamp) return false;
        lookup.CommandText = "SELECT 1 FROM mijn_password_reset WHERE token_hash=@token AND actor_type=@type AND expires_at>UTC_TIMESTAMP(6) FOR UPDATE";
        if (await lookup.ExecuteScalarAsync(ct) is null) return false;
        await StoreAsync(connection, tx, actor, password, "PasswordReset", ct);
        return true;
    }

    public async Task<bool> ChangeAsync(uint actor, string currentPassword, string password, CancellationToken ct)
    {
        await using var connection = await source.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        var current = await LockPasswordAsync(connection, tx, actor, ct);
        if (current is null || !verifier.Verify(currentPassword, current).Valid) return false;
        await StoreAsync(connection, tx, actor, password, "PasswordChanged", ct);
        return true;
    }

    private static async Task<string?> LockPasswordAsync(MySqlConnection connection, MySqlTransaction tx,
        uint actor, CancellationToken ct, bool includeEmail = false)
    {
        await using var command = new MySqlCommand($"""
            SELECT {(includeEmail ? "CONCAT(w.hash, '|', COALESCE(p.email,''))" : "w.hash")}
            FROM ah_personen_wachtwoorden w JOIN ah_personen p ON p.persoon_id=w.id
            WHERE w.id=@actor AND p.actief=1 FOR UPDATE
            """, connection, tx);
        command.Parameters.AddWithValue("@actor", actor);
        return await command.ExecuteScalarAsync(ct) as string;
    }

    private static async Task StoreAsync(MySqlConnection connection, MySqlTransaction tx, uint actor,
        string password, string action, CancellationToken ct)
    {
        const string update = "UPDATE ah_personen_wachtwoorden SET hash=@hash WHERE id=@actor;";
        await using var command = new MySqlCommand(update + """
            DELETE FROM mijn_password_reset WHERE person_id=@actor AND actor_type=@type;
            INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action)
            VALUES(UTC_TIMESTAMP(6),@type,@actor,@action);
            """, connection, tx);
        command.Parameters.AddWithValue("@actor", actor);
        command.Parameters.AddWithValue("@hash", BCrypt.Net.BCrypt.HashPassword(password, 11));
        command.Parameters.AddWithValue("@type", "choir_person");
        command.Parameters.AddWithValue("@action", action);
        await command.ExecuteNonQueryAsync(ct);
        await tx.CommitAsync(ct);
    }

    private static byte[] HashToken(string token) => SHA256.HashData(Encoding.ASCII.GetBytes(token));
}
