using System.Globalization;
using System.Security.Claims;
using Amusing.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

[EnableRateLimiting("beheer-login")]
public sealed class BeheerSigninModel(CustomAuthenticationService authentication, LoggingService audit,
    ILogger<BeheerSigninModel> logger) : PageModel
{
    public IActionResult OnGet() => LocalRedirect("/login");

    public async Task<IActionResult> OnPostAsync([FromForm] string? username, [FromForm] string? password)
    {
        username = username?.Trim();
        if (username is null or { Length: 0 or > 100 } || password is null or { Length: 0 or > 4096 })
            return LocalRedirect("/login?failed=1");

        var user = await authentication.ValidateUserAsync(username, password);
        if (user is null || string.IsNullOrWhiteSpace(user.Role))
        {
            await LogAttemptAsync("failed", $"Toegang geweigerd voor {username}");
            return LocalRedirect("/login?failed=1");
        }

        var id = user.UserId.ToString(CultureInfo.InvariantCulture);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.NameIdentifier, id),
            new("UserId", id),
            new("user_id", id)
        };
        foreach (var role in user.Role.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            claims.Add(new(ClaimTypes.Role, role));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        await LogAttemptAsync("success", $"{user.Username} is succesvol ingelogd", user.UserId);
        return LocalRedirect("/");
    }

    private async Task LogAttemptAsync(string status, string message, int? userId = null)
    {
        try { await audit.WriteUserLoginAttemptAsync(status, message, userId); }
        catch (Exception ex) { logger.LogWarning(ex, "Beheer kon een inlogpoging niet registreren."); }
    }
}
