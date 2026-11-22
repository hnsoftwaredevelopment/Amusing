using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Antiforgery;
using MySqlConnector;

namespace Amusing.Mijn.Services;

public sealed class AccountEmailChange(MySqlDataSource source, IConfiguration configuration,
    ChoirAccounts accounts, ILogger<AccountEmailChange> logger)
{
    private const string ContactPage = "/account/contactgegevens?email=";
    private const string ConfirmPage = "/account/e-mail-bevestigen";

    private async Task<string> RequestAsync(uint actor, string email, CancellationToken ct)
    {
        if (email.Length is 0 or > 80 || !new EmailAddressAttribute().IsValid(email)
            || !MailAddress.TryCreate(email, out var address) || address.Address != email)
            return "ongeldig";

        var origin = new Uri(configuration["AccountRegistration:Origin"]!, UriKind.Absolute);
        if (origin.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("The email confirmation origin must use HTTPS.");

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var tokenHash = HashToken(token);
        await using var connection = await source.OpenConnectionAsync(ct);
        await using (var tx = await connection.BeginTransactionAsync(ct))
        {
            var current = await ReadAccountAsync(connection, tx, actor, ct);
            if (current is null) return "ongeldig";
            if (string.Equals(email, current.Value.Email, StringComparison.OrdinalIgnoreCase)) return "zelfde";

            await using var duplicate = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM ah_personen WHERE email=@email AND persoon_id<>@actor)", connection, tx);
            duplicate.Parameters.AddWithValue("@email", email);
            duplicate.Parameters.AddWithValue("@actor", actor);
            if (Convert.ToBoolean(await duplicate.ExecuteScalarAsync(ct))) return "in-gebruik";

            await using var save = new MySqlCommand("""
                INSERT INTO mijn_emailwijziging(persoon_id,token_hash,current_email,new_email,credential_stamp,expires_at)
                VALUES(@actor,@token,@current,@email,@stamp,DATE_ADD(UTC_TIMESTAMP(6),INTERVAL 1 HOUR))
                ON DUPLICATE KEY UPDATE token_hash=VALUES(token_hash),current_email=VALUES(current_email),
                    new_email=VALUES(new_email),credential_stamp=VALUES(credential_stamp),expires_at=VALUES(expires_at)
                """, connection, tx);
            save.Parameters.AddWithValue("@actor", actor);
            save.Parameters.AddWithValue("@token", tokenHash);
            save.Parameters.AddWithValue("@current", current.Value.Email);
            save.Parameters.AddWithValue("@email", email);
            save.Parameters.AddWithValue("@stamp", current.Value.Stamp);
            await save.ExecuteNonQueryAsync(ct);
            await tx.CommitAsync(ct);
        }

        var url = new Uri(origin, ConfirmPage + "?token=" + token);
        var (subject, body) = (await accounts.LanguageAsync(actor, ct)) switch
        {
            "de" => ("Neue E-Mail-Adresse für Mijn Amusing bestätigen",
                $"Öffne {url} und bestätige die neue Adresse. Der Link ist eine Stunde gültig. Melde dich mit deiner bisherigen E-Mail-Adresse an, falls erforderlich."),
            "en" => ("Confirm your new Mijn Amusing email address",
                $"Open {url} and confirm the new address. The link is valid for one hour. Sign in with your current email address if asked."),
            _ => ("Bevestig je nieuwe e-mailadres voor Mijn Amusing",
                $"Open {url} en bevestig je nieuwe adres. De link is één uur geldig. Log zo nodig in met je huidige e-mailadres.")
        };
        try
        {
            await MijnMail.DeliverAsync(configuration, email, subject, body, ct, forceTestPickup: true);
            return "klaargezet";
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("E-mailbevestiging kon niet worden bezorgd ({ErrorType}).", ex.GetType().Name);
            return "mailfout";
        }
    }

    private async Task<string> ConfirmAsync(uint actor, string token, CancellationToken ct)
    {
        if (!AccountPasswords.ValidToken(token)) return "verlopen";
        await using var connection = await source.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        var current = await ReadAccountAsync(connection, tx, actor, ct);
        if (current is null) return "verlopen";

        await using var pending = new MySqlCommand("""
            SELECT new_email FROM mijn_emailwijziging
            WHERE persoon_id=@actor AND token_hash=@token AND BINARY current_email=BINARY @current
                AND credential_stamp=@stamp AND expires_at>UTC_TIMESTAMP(6) FOR UPDATE
            """, connection, tx);
        pending.Parameters.AddWithValue("@actor", actor);
        pending.Parameters.AddWithValue("@token", HashToken(token));
        pending.Parameters.AddWithValue("@current", current.Value.Email);
        pending.Parameters.AddWithValue("@stamp", current.Value.Stamp);
        if (await pending.ExecuteScalarAsync(ct) is not string email) return "verlopen";

        await using var update = new MySqlCommand("""
            UPDATE ah_personen SET email=@email WHERE persoon_id=@actor;
            DELETE FROM mijn_emailwijziging WHERE persoon_id=@actor;
            DELETE FROM mijn_password_reset WHERE person_id=@actor AND actor_type='choir_person';
            INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,changed_fields)
            VALUES(UTC_TIMESTAMP(6),'choir_person',@actor,'PersonalContactChanged','Email');
            """, connection, tx);
        update.Parameters.AddWithValue("@actor", actor);
        update.Parameters.AddWithValue("@email", email);
        try { await update.ExecuteNonQueryAsync(ct); }
        catch (MySqlException ex) when (ex.Number == 1062) { return "in-gebruik"; }
        await tx.CommitAsync(ct);
        return "gewijzigd";
    }

    private static async Task<(string Email, string Stamp)?> ReadAccountAsync(MySqlConnection connection,
        MySqlTransaction tx, uint actor, CancellationToken ct)
    {
        await using var account = new MySqlCommand("""
            SELECT COALESCE(p.email,''),w.hash FROM ah_personen p JOIN ah_personen_wachtwoorden w ON w.id=p.persoon_id
            WHERE p.persoon_id=@actor AND p.actief=1 FOR UPDATE
            """, connection, tx);
        account.Parameters.AddWithValue("@actor", actor);
        await using var reader = await account.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? (reader.GetString(0), ChoirAccounts.Stamp(reader.GetString(1))) : null;
    }

    private static byte[] HashToken(string token) => SHA256.HashData(Convert.FromHexString(token));

    private static uint? Actor(ClaimsPrincipal user) => user.FindFirstValue("actor_type") == "choir_person"
        && uint.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public static void Map(WebApplication app)
    {
        app.MapPost("/account/e-mail-aanvragen", async (HttpContext context, IAntiforgery csrf, AccountEmailChange changes) =>
        {
            if (Actor(context.User) is not { } actor) return Results.Forbid();
            if (!await csrf.IsRequestValidAsync(context)) return Results.BadRequest();
            var ct = context.RequestAborted;
            var form = await context.Request.ReadFormAsync(ct);
            return Results.LocalRedirect(ContactPage + await changes.RequestAsync(actor, form["email"].ToString().Trim(), ct));
        }).RequireAuthorization().RequireRateLimiting("login");

        app.MapPost("/account/e-mail-wijziging-bevestigen", async (HttpContext context, IAntiforgery csrf, AccountEmailChange changes) =>
        {
            if (Actor(context.User) is not { } actor) return Results.Forbid();
            if (!await csrf.IsRequestValidAsync(context)) return Results.BadRequest();
            var ct = context.RequestAborted;
            var form = await context.Request.ReadFormAsync(ct);
            return Results.LocalRedirect(ContactPage + await changes.ConfirmAsync(actor, form["token"].ToString(), ct));
        }).RequireAuthorization().RequireRateLimiting("login");
    }
}
