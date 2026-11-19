using System.Security.Claims;
using System.Threading.RateLimiting;
using Amusing.Authentication;
using Amusing.Authentication.MySql;
using Amusing.Mijn.Components;
using Amusing.Mijn.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);
if (builder.Configuration["Syncfusion:License"] is { Length: > 0 } pdfLicense)
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(pdfLicense);
var protection = builder.Services.AddDataProtection().SetApplicationName("Amusing.Mijn");
if (builder.Configuration["Mijn:DataProtectionPath"] is string keyPath)
    protection.PersistKeysToFileSystem(new DirectoryInfo(keyPath));
builder.Services.AddRazorComponents();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddSingleton(_ => new MySqlDataSource(builder.Configuration.GetConnectionString("Mijn") ?? throw new InvalidOperationException("Configure ConnectionStrings:Mijn using user secrets or environment variables.")));
builder.Services.AddScoped<ChoirAccounts>();
builder.Services.AddScoped<SiteLanguage>();
builder.Services.AddScoped<AccountRegistration>();
builder.Services.AddScoped<AccountPasswords>();
builder.Services.AddScoped<PersonalContacts>();
builder.Services.AddScoped<AccountEmailChange>();
builder.Services.AddScoped<ChoirAudit>();
builder.Services.AddScoped<SupportAccounts>();
builder.Services.AddScoped<ChoirProfiles>();
builder.Services.AddScoped<ChoirImagesStore>();
builder.Services.AddScoped<ChoirPeople>();
builder.Services.AddSingleton<IFestivalClock,FestivalClock>();
builder.Services.AddScoped<ChoirEnrollments>();
builder.Services.AddHostedService<EnrollmentMailWorker>();
builder.Services.AddHostedService<WaitlistPlacementWorker>();
builder.WebHost.ConfigureKestrel(options=>options.Limits.MaxRequestBodySize=20*1024*1024);
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options=>options.MultipartBodyLengthLimit=20*1024*1024);
builder.Services.AddScoped<IChoirPasswordStore>(sp => new MySqlChoirPasswordStore(sp.GetRequiredService<MySqlDataSource>()));
builder.Services.AddScoped<ChoirPasswordVerifier>();
builder.Services.AddScoped<ChoirCredentialService>();
builder.Services.AddScoped<MySqlChoirAuditLog>(sp => new MySqlChoirAuditLog(sp.GetRequiredService<MySqlDataSource>()));
builder.Services.AddAntiforgery();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
    options.KnownProxies.Add(IPAddress.Loopback);
    options.KnownProxies.Add(IPAddress.IPv6Loopback);
    options.ForwardLimit = 1;
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "Amusing.Mijn.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.LoginPath = "/account/login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = false;
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
    options.Events.OnValidatePrincipal = async context =>
    {
        var services = context.HttpContext.RequestServices;
        var ct = context.HttpContext.RequestAborted;
        var current = context.Principal.IsAdministrator()
            ? await services.GetRequiredService<SupportAccounts>().IsCurrentAsync(context.Principal!, ct)
            : await services.GetRequiredService<ChoirAccounts>().IsCurrentAsync(context.Principal!, ct);
        if (!current)
        {
            var actor = uint.TryParse(context.Principal!.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? (uint?)id : null;
            if (!context.Principal.IsAdministrator()) await context.HttpContext.RequestServices.GetRequiredService<MySqlChoirAuditLog>().RecordAsync(ChoirAuditAction.SessionInvalidated, actor, context.HttpContext.RequestAborted);
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
app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); app.UseHttpsRedirection(); }
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    if (app.Configuration.GetValue<bool>("Mijn:IsTestEnvironment")) context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
    context.Response.Headers["Referrer-Policy"] = "same-origin";
    context.Response.Headers.CacheControl = "no-store";
    await next();
});
app.UseAuthentication();
app.Use(async (context, next) =>
{
    if (context.User.IsAdministrator())
    {
        var path = context.Request.Path;
        if (path.StartsWithSegments("/koor", out var remaining))
        {
            var groupSegment = remaining.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!uint.TryParse(groupSegment, out var group) ||
                context.User.FindFirstValue(SupportMode.GroupClaim) != group.ToString())
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
        }
        if (path.StartsWithSegments("/account/uitnodigingen") || path.StartsWithSegments("/uitnodigingen"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }
    }
    await next();
});
app.UseAuthorization();
app.UseRateLimiter();
app.UseAntiforgery();
app.MapPost("/account/sign-in", async (HttpContext context, IAntiforgery antiforgery, ChoirAccounts accounts, ChoirCredentialService credentials, IChoirPasswordStore store, MySqlChoirAuditLog audit, ILoggerFactory logs) =>
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
        await audit.RecordAsync(ChoirAuditAction.LoginRejected, account?.Id, context.RequestAborted);
        logs.CreateLogger("ChoirLogin").LogInformation("Choir login rejected");
        return Results.LocalRedirect("/account/login?failed=1");
    }
    var hash = await store.ReadHashAsync(account.Id, context.RequestAborted);
    // Revalidate after migration before binding the session to the current credential.
    if (hash is null || !BCrypt.Net.BCrypt.Verify(password, hash)) return Results.LocalRedirect("/account/login?failed=1");
    var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()), new Claim(ClaimTypes.Name, account.Name), new Claim("actor_type", "choir_person"), new Claim("credential_stamp", ChoirAccounts.Stamp(hash)) }, CookieAuthenticationDefaults.AuthenticationScheme));
    if (!await accounts.IsCurrentAsync(principal, context.RequestAborted)) return Results.LocalRedirect("/account/login?failed=1");
    await audit.RecordAsync(ChoirAuditAction.LoginSucceeded, account.Id, context.RequestAborted);
    await context.SignInAsync(principal, new AuthenticationProperties { IsPersistent = false });
    logs.CreateLogger("ChoirLogin").LogInformation("Choir person {PersonId} logged in", account.Id);
    return Results.LocalRedirect("/");
}).RequireRateLimiting("login");
app.MapPost("/account/logout", async (HttpContext context, IAntiforgery antiforgery, MySqlChoirAuditLog audit) =>
{
    try { await antiforgery.ValidateRequestAsync(context); }
    catch (AntiforgeryValidationException) { return Results.BadRequest(); }
    if (!context.User.IsAdministrator()) await audit.RecordAsync(ChoirAuditAction.LoggedOut, uint.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)!), context.RequestAborted);
    await context.SignOutAsync();
    return Results.LocalRedirect("/account/login");
}).RequireAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "ok", application = "Amusing.Mijn", version = "dev-enrollment-1" }));
app.MapGet("/robots.txt", () => Results.Text("User-agent: *\nDisallow: /\n", "text/plain"));
app.MapStaticAssets();
app.MapPost("/account/taal", async (HttpContext context, IAntiforgery csrf, ChoirAccounts accounts) =>
{
    if (!await csrf.IsRequestValidAsync(context)) return Results.BadRequest();
    var form = await context.Request.ReadFormAsync(context.RequestAborted);
    var language = form["taal"].ToString();
    if (!SiteLanguage.IsSupported(language)) return Results.BadRequest();
    var target = form["terug"].ToString();
    if (target.Length == 0 || target[0] != '/' || target.StartsWith("//") ||
        target.Contains('\\') || target.Any(char.IsControl))
        target = "/";

    if (context.User.FindFirstValue("actor_type") == "choir_person" &&
        uint.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var actor))
        await accounts.SaveLanguageAsync(actor, language, context.RequestAborted);

    context.Response.Cookies.Append(SiteLanguage.CookieName, language, new CookieOptions
    {
        HttpOnly = true, Secure = context.Request.IsHttps, SameSite = SameSiteMode.Lax,
        MaxAge = TimeSpan.FromDays(365), Path = "/"
    });
    return Results.LocalRedirect(target);
});

app.MapChoirImages();
app.MapChoirPeople();
app.MapSupport();
AccountRegistration.Map(app);
AccountEmailChange.Map(app);
app.MapAccountPasswords();
app.MapParticipationNotes();
app.MapRazorComponents<App>();
app.Run();
public partial class Program { }
