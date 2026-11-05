using System.Security.Claims;
using System.Threading.RateLimiting;
using Amusing.Authentication;
using Amusing.Authentication.MySql;
using Amusing.Mijn.Components;
using Amusing.Mijn.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddSingleton(_ => new MySqlDataSource(builder.Configuration.GetConnectionString("Mijn") ?? throw new InvalidOperationException("Configure ConnectionStrings:Mijn using user secrets or environment variables.")));
builder.Services.AddScoped<ChoirAccounts>();
builder.Services.AddScoped<IChoirPasswordStore>(sp => new MySqlChoirPasswordStore(sp.GetRequiredService<MySqlDataSource>()));
builder.Services.AddScoped<ChoirPasswordVerifier>();
builder.Services.AddScoped<ChoirCredentialService>();
builder.Services.AddAntiforgery();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "Amusing.Mijn.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.LoginPath = "/account/login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = false;
    options.Events.OnValidatePrincipal = async context =>
    {
        if (!await context.HttpContext.RequestServices.GetRequiredService<ChoirAccounts>().IsCurrentAsync(context.Principal!, context.HttpContext.RequestAborted))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
        }
    };
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
var dummyHash = BCrypt.Net.BCrypt.HashPassword("Synthetic-timing-fixture", 11);
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); app.UseHttpsRedirection(); }
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "same-origin";
    context.Response.Headers.CacheControl = "no-store";
    await next();
});
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseAntiforgery();
app.MapPost("/account/sign-in", async (HttpContext context, IAntiforgery antiforgery, ChoirAccounts accounts, ChoirCredentialService credentials, IChoirPasswordStore store, ILoggerFactory logs) =>
{
    try { await antiforgery.ValidateRequestAsync(context); }
    catch (AntiforgeryValidationException) { return Results.BadRequest(); }
    var form = await context.Request.ReadFormAsync(context.RequestAborted);
    var email = form["email"].ToString().Trim();
    var password = form["password"].ToString();
    if (email.Length > 80 || password.Length is 0 or > 4096) return Results.LocalRedirect("/account/login?failed=1");
    var account = await accounts.FindAsync(email, context.RequestAborted);
    var valid = account is not null ? await credentials.VerifyAsync(account.Id, password, context.RequestAborted) : BCrypt.Net.BCrypt.Verify(password, dummyHash);
    if (account is null || !valid)
    {
        logs.CreateLogger("ChoirLogin").LogInformation("Choir login rejected");
        return Results.LocalRedirect("/account/login?failed=1");
    }
    var hash = await store.ReadHashAsync(account.Id, context.RequestAborted);
    // Revalidate after migration before binding the session to the current credential.
    if (hash is null || !BCrypt.Net.BCrypt.Verify(password, hash)) return Results.LocalRedirect("/account/login?failed=1");
    var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()), new Claim(ClaimTypes.Name, account.Name), new Claim("actor_type", "choir_person"), new Claim("credential_stamp", ChoirAccounts.Stamp(hash)) }, CookieAuthenticationDefaults.AuthenticationScheme));
    if (!await accounts.IsCurrentAsync(principal, context.RequestAborted)) return Results.LocalRedirect("/account/login?failed=1");
    await context.SignInAsync(principal, new AuthenticationProperties { IsPersistent = false });
    logs.CreateLogger("ChoirLogin").LogInformation("Choir person {PersonId} logged in", account.Id);
    return Results.LocalRedirect("/");
}).RequireRateLimiting("login");
app.MapPost("/account/logout", async (HttpContext context, IAntiforgery antiforgery) =>
{
    try { await antiforgery.ValidateRequestAsync(context); }
    catch (AntiforgeryValidationException) { return Results.BadRequest(); }
    await context.SignOutAsync();
    return Results.LocalRedirect("/account/login");
}).RequireAuthorization();
app.MapStaticAssets();
app.MapRazorComponents<App>();
app.Run();
public partial class Program { }
