using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Amusing.Services;

public class LogoutModel(LoggingService audit, ILogger<LogoutModel> logger) : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            try { await audit.WriteUserLogoutAttemptAsync("success", $"{User.Identity.Name} is uitgelogd"); }
            catch (Exception ex) { logger.LogWarning(ex, "Beheer kon het uitloggen niet registreren."); }
        }
        await HttpContext.SignOutAsync( CookieAuthenticationDefaults.AuthenticationScheme );
        return LocalRedirect("/");
    }
}
