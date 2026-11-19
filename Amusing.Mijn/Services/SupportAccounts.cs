using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlConnector;

namespace Amusing.Mijn.Services;

public sealed class SupportAccounts(MySqlDataSource source)
{
    private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString(), 11);
    public sealed record Account(uint Id, string Name, string Hash, bool Legacy);

    public async Task<Account?> VerifyAsync(string username, string password, CancellationToken ct)
    {
        var account = await FindAsync("username", username, ct);
        try
        {
            if (account is { Legacy: true })
            {
                _ = BCrypt.Net.BCrypt.Verify(password, DummyHash);
                var digest = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(password))).ToLowerInvariant();
                return CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(digest),
                    Encoding.ASCII.GetBytes(account.Hash)) ? account : null;
            }

            return BCrypt.Net.BCrypt.Verify(password, account?.Hash ?? DummyHash) ? account : null;
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return null;
        }
    }

    private async Task<Account?> FindAsync(string column, object value, CancellationToken ct)
    {
        await using var cmd = source.CreateCommand($"""
            SELECT user_id, username, COALESCE(NULLIF(PasswordHash,''), password), NULLIF(PasswordHash,'') IS NULL FROM ah_beheer
            WHERE {column}=@value AND NULLIF(TRIM(role),'') IS NOT NULL AND COALESCE(NULLIF(PasswordHash,''), NULLIF(password,'')) IS NOT NULL
            LIMIT 2
            """);
        Add(cmd, "@value", value);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        var account = new Account(Convert.ToUInt32(reader.GetValue(0)), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3));
        return await reader.ReadAsync(ct) ? null : account;
    }

    public async Task<bool> IsCurrentAsync(ClaimsPrincipal principal, CancellationToken ct)
    {
        if (!uint.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return false;
        var account = await FindAsync("user_id", id, ct);
        return account is not null && principal.FindFirstValue("credential_stamp") == ChoirAccounts.Stamp(account.Hash);
    }

    public async Task<List<ChoirSummary>> GroupsAsync(CancellationToken ct)
    {
        await using var cmd = source.CreateCommand("SELECT zanggroep_id, naam FROM ah_zanggroepen ORDER BY naam, zanggroep_id");
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var groups = new List<ChoirSummary>();
        while (await reader.ReadAsync(ct))
            groups.Add(new(reader.GetFieldValue<uint>(0), reader.GetString(1)));
        return groups;
    }

    private static void Add(System.Data.Common.DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    public async Task<bool> GroupExistsAsync(uint group, CancellationToken ct)
    {
        await using var cmd = source.CreateCommand("SELECT EXISTS(SELECT 1 FROM ah_zanggroepen WHERE zanggroep_id=@id)");
        Add(cmd, "@id", group);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)) == 1;
    }
}

public static class SupportEndpoints
{
    public static void MapSupport(this WebApplication app)
    {
        app.MapPost("/account/beheer-inloggen", async (HttpContext context, IAntiforgery csrf, SupportAccounts accounts) =>
        {
            if (!await csrf.IsRequestValidAsync(context)) return Results.BadRequest();
            var ct = context.RequestAborted;
            var form = await context.Request.ReadFormAsync(ct);
            var name = form["username"].ToString().Trim();
            var password = form["password"].ToString();
            if (name.Length is 0 or > 100 || password.Length is 0 or > 4096)
                return Results.LocalRedirect("/account/beheer?failed=1");
            var account = await accounts.VerifyAsync(name, password, ct);
            if (account is null) return Results.LocalRedirect("/account/beheer?failed=1");

            var identity = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.Name),
                new Claim("actor_type", SupportMode.ActorType),
                new Claim("credential_stamp", ChoirAccounts.Stamp(account.Hash))
            ], CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignInAsync(new ClaimsPrincipal(identity));
            return Results.LocalRedirect("/");
        }).RequireRateLimiting("login");

        app.MapPost("/ondersteuning/koor", async (HttpContext context, IAntiforgery csrf, SupportAccounts accounts) =>
        {
            if (!context.User.IsAdministrator()) return Results.Forbid();
            if (!await csrf.IsRequestValidAsync(context)) return Results.BadRequest();
            var form = await context.Request.ReadFormAsync(context.RequestAborted);
            var identity = new ClaimsIdentity(context.User.Identity!);
            if (identity.FindFirst(SupportMode.GroupClaim) is { } old)
                identity.RemoveClaim(old);
            var target = "/";
            if (uint.TryParse(form["group"], out var group) && await accounts.GroupExistsAsync(group, context.RequestAborted))
            {
                identity.AddClaim(new Claim(SupportMode.GroupClaim, group.ToString()));
                target = $"/koor/{group}";
            }

            var session = await context.AuthenticateAsync();
            await context.SignInAsync(new ClaimsPrincipal(identity), session.Properties);
            return Results.LocalRedirect(target);
        }).RequireAuthorization();
    }
}
