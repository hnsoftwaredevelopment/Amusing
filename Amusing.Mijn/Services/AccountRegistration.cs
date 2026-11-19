using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Antiforgery;
using MySqlConnector;

namespace Amusing.Mijn.Services;

public sealed class AccountRegistration(MySqlDataSource source, IConfiguration configuration,
    ILogger<AccountRegistration> logger)
{
    public async Task RequestAsync(string first, string last, string email, CancellationToken ct)
    {
        if (!configuration.GetValue<bool>("AccountRegistration:Enabled"))
            return;

        var origin = new Uri(configuration["AccountRegistration:Origin"]!, UriKind.Absolute);
        if (origin.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("The activation origin must use HTTPS.");

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var secret = HashToken(token);


        await using var connection = await source.OpenConnectionAsync(ct);
        await using var command = new MySqlCommand("""
            INSERT INTO mijn_account_activation(token_hash,email,first_name,last_name,expires_at)
            SELECT @token,@email,@first,@last,DATE_ADD(UTC_TIMESTAMP(6),INTERVAL 24 HOUR)
            WHERE NOT EXISTS (SELECT 1 FROM ah_personen WHERE email=@email)
            """, connection);
        command.Parameters.AddWithValue("@token", secret);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@first", first);
        command.Parameters.AddWithValue("@last", last);

        if (await command.ExecuteNonQueryAsync(ct) == 0)
            return;

        try
        {
            var url = new Uri(origin, "/account/activeren?token=" + token);
            await MijnMail.DeliverAsync(configuration, email, "Activeer je Mijn Amusing-account",
                $"Open {url} en klik op Account activeren. De link is 24 uur geldig.", ct, anonymousAccountMail: true);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("Accountactivatiemail kon niet worden bezorgd ({ErrorType}).", ex.GetType().Name);
        }
    }

    public async Task<bool> ActivateAsync(string token, string password, CancellationToken ct)
    {
        if (!configuration.GetValue<bool>("AccountRegistration:Enabled")) return false;
        if (token.Length != 64 || !token.All(Uri.IsHexDigit))
            return false;

        await using var connection = await source.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        var hash = HashToken(token);
        await using var create = new MySqlCommand("""
            INSERT INTO ah_personen(voornaam,achternaam,email,actief)
            SELECT first_name,last_name,email,1 FROM mijn_account_activation
            WHERE token_hash=@token AND expires_at>UTC_TIMESTAMP(6)
            """, connection, tx);
        create.Parameters.AddWithValue("@token", hash);
        try
        {
            if (await create.ExecuteNonQueryAsync(ct) != 1)
                return false;
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return false;
        }

        await using var complete = new MySqlCommand("""
            INSERT INTO ah_personen_wachtwoorden(id,hash)
            VALUES(@actor,@password);
            DELETE FROM mijn_account_activation WHERE email=(SELECT email FROM ah_personen WHERE persoon_id=@actor);
            INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action)
            VALUES(UTC_TIMESTAMP(6),'choir_person',@actor,'AccountActivated');
            """, connection, tx);
        complete.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(password, 11));
        complete.Parameters.AddWithValue("@actor", create.LastInsertedId);

        await complete.ExecuteNonQueryAsync(ct);
        await tx.CommitAsync(ct);
        return true;
    }

    private static byte[] HashToken(string token) => SHA256.HashData(Encoding.ASCII.GetBytes(token));

    public static void Map(WebApplication app)
    {
        app.MapPost("/account/registratie-aanvragen", async (HttpContext context, IAntiforgery csrf, AccountRegistration accounts) =>
        {
            if (!await csrf.IsRequestValidAsync(context)) return Results.BadRequest();
            if (context.User.Identity?.IsAuthenticated == true) return Results.Forbid();
            var form = await context.Request.ReadFormAsync(context.RequestAborted);
            var first = form["first"].ToString().Trim();
            var last = form["last"].ToString().Trim();
            var email = form["email"].ToString().Trim();


            if (first.Length is 0 or > 40 || last.Length is 0 or > 50 ||
                email.Length is 0 or > 80 || !new EmailAddressAttribute().IsValid(email) ||
                !MailAddress.TryCreate(email, out var address) || address.Address != email)
                return Results.LocalRedirect("/account/registreren?status=ongeldig");

            await accounts.RequestAsync(first, last, email, context.RequestAborted);
            return Results.LocalRedirect("/account/registreren?status=ontvangen");
        }).RequireRateLimiting("login");

        app.MapPost("/account/activering-bevestigen", async (HttpContext context, IAntiforgery csrf, AccountRegistration accounts) =>
        {
            if (!await csrf.IsRequestValidAsync(context)) return Results.BadRequest();
            var form = await context.Request.ReadFormAsync(context.RequestAborted);
            var token = form["token"].ToString();
            var password = form["password"].ToString();
            if (password.Length < 12 || Encoding.UTF8.GetByteCount(password) > 72 || password != form["repeat"].ToString())
                return token.Length == 64 && token.All(Uri.IsHexDigit)
                    ? Results.LocalRedirect("/account/activeren?status=wachtwoord&token=" + token)
                    : Results.LocalRedirect("/account/activeren?status=ongeldig");
            var activated = await accounts.ActivateAsync(token, password, context.RequestAborted);
            return Results.LocalRedirect(activated ? "/account/login?activated=1" : "/account/activeren?status=ongeldig");
        }).RequireRateLimiting("login");
    }
}
