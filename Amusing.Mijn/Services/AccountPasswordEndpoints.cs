using System.Net.Mail;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;

namespace Amusing.Mijn.Services;

public static class AccountPasswordEndpoints
{
    public static void MapAccountPasswords(this WebApplication app)
    {
        app.MapPost("/account/wachtwoord/aanvragen", async (HttpContext context, IAntiforgery csrf,
            AccountPasswords passwords, IConfiguration configuration, SiteLanguage siteLanguage) =>
        {
            if (!await csrf.IsRequestValidAsync(context)) return Results.BadRequest();
            if (!configuration.GetValue<bool>("AccountPasswords:ResetEnabled")) return Results.NotFound();

            if (context.User.Identity?.IsAuthenticated == true) return Results.Forbid();
            var form = await context.Request.ReadFormAsync(context.RequestAborted);
            var beheer = form["beheer"] == "1";
            var suffix = beheer ? "&beheer=1" : "";
            var username = form["username"].ToString().Trim();
            var email = form["email"].ToString().Trim();
            if ((beheer && (username.Length is 0 or > 100)) || email.Length is 0 or > 80 || !MailAddress.TryCreate(email, out var address) || address.Address != email)
                return Results.LocalRedirect("/account/wachtwoord-vergeten?status=ongeldig" + suffix);

            await passwords.RequestAsync(email, context.RequestAborted, beheer ? username : null, await siteLanguage.GetAsync());
            return Results.LocalRedirect("/account/wachtwoord-vergeten?status=ontvangen" + suffix);
        }).RequireRateLimiting("login");

        app.MapPost("/account/wachtwoord/instellen", async (HttpContext context, IAntiforgery csrf,
            AccountPasswords passwords, IConfiguration configuration) =>
        {
            if (!await csrf.IsRequestValidAsync(context)) return Results.BadRequest();
            if (!configuration.GetValue<bool>("AccountPasswords:ResetEnabled")) return Results.NotFound();

            var form = await context.Request.ReadFormAsync(context.RequestAborted);
            var beheer = form["beheer"] == "1";
            var suffix = beheer ? "&beheer=1" : "";
            var token = form["token"].ToString();
            var password = form["password"].ToString();
            if (!AccountPasswords.ValidToken(token))
                return Results.LocalRedirect("/account/wachtwoord-instellen?status=link" + suffix);
            if (!AccountPasswords.ValidPassword(password, form["repeat"].ToString()))
                return Results.LocalRedirect("/account/wachtwoord-instellen?status=ongeldig&token=" + token + suffix);
            if (!await passwords.ResetAsync(token, password, context.RequestAborted, beheer))
                return Results.LocalRedirect("/account/wachtwoord-instellen?status=link" + suffix);

            await context.SignOutAsync();
            return Results.LocalRedirect(beheer ? "/account/beheer?gewijzigd=1" : "/account/login?wachtwoord=gewijzigd");
        }).RequireRateLimiting("login");

        app.MapPost("/account/wachtwoord/wijzigen", async (HttpContext context, IAntiforgery csrf, AccountPasswords passwords) =>
        {
            if (context.User.FindFirstValue("actor_type") != "choir_person" ||
                !uint.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var actor))
                return Results.Forbid();
            if (!await csrf.IsRequestValidAsync(context)) return Results.BadRequest();

            var form = await context.Request.ReadFormAsync(context.RequestAborted);
            var password = form["password"].ToString();
            var current = form["current"].ToString();
            if (current.Length is 0 or > 4096 || !AccountPasswords.ValidPassword(password, form["repeat"].ToString()) ||
                !await passwords.ChangeAsync(actor, current, password, context.RequestAborted))
                return Results.LocalRedirect("/account/wachtwoord-wijzigen?status=ongeldig");

            await context.SignOutAsync();
            return Results.LocalRedirect("/account/login?wachtwoord=gewijzigd");
        }).RequireAuthorization().RequireRateLimiting("login");
    }
}
