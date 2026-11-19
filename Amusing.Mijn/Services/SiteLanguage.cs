using System.Security.Claims;

namespace Amusing.Mijn.Services;

public sealed class SiteLanguage(IHttpContextAccessor accessor, ChoirAccounts accounts)
{
    public const string CookieName = "Amusing.Mijn.Language";
    private Task<string>? current;

    public static bool IsSupported(string? value) => value is "nl" or "de" or "en";

    public Task<string> GetAsync() => current ??= ReadAsync();

    private async Task<string> ReadAsync()
    {
        var context = accessor.HttpContext!;
        if (context.User.FindFirstValue("actor_type") == "choir_person" &&
            uint.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var actor))
            return await accounts.LanguageAsync(actor, context.RequestAborted);

        var language = context.Request.Cookies[CookieName];
        return IsSupported(language) ? language! : "nl";
    }
}
