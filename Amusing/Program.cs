using System.Globalization;
using System.Threading.RateLimiting;

using Amusing.Components.Account;
using Amusing.Data;
using Amusing.Helpers;
using Amusing.Models;
using Amusing.Security;
using Amusing.Services;
using Amusing.Services.Legacy;

using Blazored.SessionStorage;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

using Syncfusion.Blazor;

// Register Syncfusion
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
    "Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZfeHRRR2ZeUEVyX0FWYEg="
);

var builder = WebApplication.CreateBuilder(args);

// Debug: dump all config providers
var root = (IConfigurationRoot)builder.Configuration;
//foreach (var provider in root.Providers)
//{
//    Console.WriteLine("Provider: " + provider);
//}

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("=== ENVIRONMENT VARIABLES CHECK ===");
logger.LogInformation($"DOTNET_ENVIRONMENT: {Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}");
logger.LogInformation($"ConnectionStrings__DefaultConnection: {(string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")) ? "NOT FOUND" : "FOUND")}");
logger.LogInformation($"DefaultConnection: {(string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DefaultConnection")) ? "NOT FOUND" : "FOUND")}");
logger.LogInformation("====================================");

// ---------------------------------------------------------
// 1. Configuration loading (correct load order)
// ---------------------------------------------------------
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.PasswordRecovery.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

var defaultConn = builder.Configuration.GetValue<string>("DefaultConnection");
//Console.WriteLine($"DefaultConnection={defaultConn}");

// -------------------------------
// 2. Retrieve connectionstring (sanity check)
// -------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var config = builder.Configuration;
logger.LogInformation("=== CONFIGURATION CHECK ===");
logger.LogInformation($"GetConnectionString('DefaultConnection'): {(string.IsNullOrWhiteSpace(config.GetConnectionString("DefaultConnection")) ? "NOT FOUND" : "FOUND")}");
logger.LogInformation($"Direct ['DefaultConnection']: {(string.IsNullOrWhiteSpace(config["DefaultConnection"]) ? "NOT FOUND" : "FOUND")}");
logger.LogInformation($"Full path ['ConnectionStrings:DefaultConnection']: {(string.IsNullOrWhiteSpace(config["ConnectionStrings:DefaultConnection"]) ? "NOT FOUND" : "FOUND")}");
logger.LogInformation("===========================");

// Dump na laden
//Console.WriteLine("=== CONFIG DUMP ===");
//Console.WriteLine("ENV: " + builder.Environment.EnvironmentName);
//Console.WriteLine("ConnectionString: " + builder.Configuration.GetConnectionString("DefaultConnection"));
//Console.WriteLine("====================");

// Debug output
//Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
//Console.WriteLine($"DB Connection: {(string.IsNullOrWhiteSpace(connectionString) ? "NOT FOUND" : "FOUND")}");

// ---------------------------------------------------------
// 2. Email settings configuration
// ---------------------------------------------------------
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<EmailSettings>>().Value);

var smtpPass = builder.Configuration["EmailSettings:SmtpPass"];
//Console.WriteLine($"SMTP Password Loaded: {smtpPass ?? "NOT FOUND"}");

// ---------------------------------------------------------
// 3. Culture settings
// ---------------------------------------------------------
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("nl-NL");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("nl-NL");

// ---------------------------------------------------------
// 4. Service registrations (YOUR original list restored)
// ---------------------------------------------------------
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();

// Your services
builder.Services.AddScoped<LoggingService>();
builder.Services.AddScoped<GenericDataService>();
builder.Services.AddScoped<CountryService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<EditionService>();
builder.Services.AddScoped<EmailAddressesService>();
builder.Services.AddScoped<FestivalService>();
builder.Services.AddScoped<GenreService>();
builder.Services.AddScoped<GitHubService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddScoped<RegistrationAvailabilityService>();
builder.Services.AddScoped<RegistrationEditService>();
builder.Services.AddScoped<WaitlistService>();
builder.Services.AddScoped<StageService>();
builder.Services.AddScoped<StageTypeService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<VolunteerService>();
builder.Services.AddScoped<FieldMappingService>();
builder.Services.AddScoped<UserContextHelper>();
builder.Services.AddScoped<PlanningService>();
builder.Services.AddScoped<MobilePlanningService>();
builder.Services.AddScoped<ToastService>();

builder.Services.AddBlazoredSessionStorage();

// Email + HTTP related
//builder.Services.AddScoped<TransipMailingService>();
builder.Services.AddScoped<MailingService>();
// Disabled until the explicit schema migration and per-environment rollout are completed.
builder.Services.AddSingleton(sp => new Amusing.Services.MailQueue.MailQueueStore(
    sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")!,
    logger: sp.GetRequiredService<ILogger<Amusing.Services.MailQueue.MailQueueStore>>()));
builder.Services.AddSingleton<Amusing.Services.MailQueue.IQueueMailTransport>(sp =>
    new Amusing.Services.MailQueue.SmtpQueueTransport(sp.GetRequiredService<EmailSettings>()));
builder.Services.AddHostedService<Amusing.Services.MailQueue.MailQueueWorker>();
builder.Services.AddScoped<IParticipationInvoiceSender>(sp => sp.GetRequiredService<MailingService>());
builder.Services.AddScoped<IParticipationInvoiceData, ParticipationInvoiceData>();
builder.Services.AddScoped<ParticipationInvoiceService>();
builder.Services.AddSingleton<IMailingLogger, ConsoleMailingLogger>();

// Identity
builder.Services.AddScoped<IEmailSender<ApplicationUser>, SmtpEmailSender<ApplicationUser>>();
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, LegacyPasswordHasher>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Amusing.Beheer.Session.v2";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.LoginPath = "/login";
    });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("beheer-recovery", context => HttpMethods.IsGet(context.Request.Method)
        ? RateLimitPartition.GetNoLimiter("recovery-page")
        : RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "onbekend",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 12, Window = TimeSpan.FromMinutes(1) }));
    options.AddPolicy("beheer-login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "onbekend",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 12, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddScoped<CustomAuthenticationService>();
builder.Services.AddSingleton<System.Data.Common.DbDataSource>(sp =>
    new MySqlConnector.MySqlDataSource(sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")!));
builder.Services.AddScoped<Amusing.Authentication.MySql.BeheerPasswordRecoveryStore>();
builder.Services.AddScoped<BeheerPasswordRecoveryService>(sp=>
{
    var fallback=sp.GetRequiredService<EmailSettings>();
    var settings=BeheerPasswordRecoveryService.ResolveMailSettings(builder.Configuration,fallback);
    var transport=ReferenceEquals(settings,fallback)?sp.GetRequiredService<Amusing.Services.MailQueue.IQueueMailTransport>()
        :new Amusing.Services.MailQueue.SmtpQueueTransport(settings);
    return new BeheerPasswordRecoveryService(sp.GetRequiredService<Amusing.Authentication.MySql.BeheerPasswordRecoveryStore>(),builder.Configuration,
        sp.GetRequiredService<IHostEnvironment>(),settings,transport,sp.GetRequiredService<ILogger<BeheerPasswordRecoveryService>>());
});
builder.Services.AddScoped<ProtectedSessionStorage>();

builder.Services.AddAuthorizationCore();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ---------------------------------------------------------
// 5. Database configuration
// ---------------------------------------------------------
var conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(conn, ServerVersion.AutoDetect(conn))
);

// ---------------------------------------------------------
// 6. Syncfusion
// ---------------------------------------------------------
builder.Services.AddSyncfusionBlazor();
builder.Services.AddSingleton(typeof(ISyncfusionStringLocalizer), typeof(SyncfusionLocalizer));

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// ---------------------------------------------------------
// Build & middleware pipeline
// ---------------------------------------------------------
var app = builder.Build();

app.UseForwardedHeaders( new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
} );

app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/mobile/current-performances", async (MobilePlanningService mobilePlanningService) =>
{
    var planning = await mobilePlanningService.GetCurrentPlanningAsync();
    return planning is null ? Results.NotFound() : Results.Ok(planning);
})
.AllowAnonymous()
.WithName("GetCurrentMobilePerformances");

app.MapGet("/api/mobile/current-festival", async (MobilePlanningService mobilePlanningService) =>
{
    var planning = await mobilePlanningService.GetCurrentPlanningAsync();
    return planning is null ? Results.NotFound() : Results.Ok(planning.Festival);
})
.AllowAnonymous()
.WithName("GetCurrentMobileFestival");

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

