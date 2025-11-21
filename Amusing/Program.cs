using System.Globalization;

using Amusing.Components.Account;
using Amusing.Data;
using Amusing.Helpers;
using Amusing.Models;
using Amusing.Security;
using Amusing.Services;
using Amusing.Services.Legacy;

using Blazored.SessionStorage;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Syncfusion.Blazor;

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense( "Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZfeHRRR2ZeUEVyX0FWYEg=" );

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// --- Secure configuration setup ---
builder.Configuration
    .SetBasePath( Directory.GetCurrentDirectory() )
    .AddJsonFile( "appsettings.json", optional: false, reloadOnChange: true )
    .AddJsonFile( $"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true )
    .AddUserSecrets<Program>( optional: true )
    .AddEnvironmentVariables();
//.AddUserSecrets<Program>( optional: true )
//.AddEnvironmentVariables();

// Make sure TransipMailingService uses the same EmailSettings
//builder.Services.AddSingleton<TransipMailingService>( sp =>
//{
//    var settings = sp.GetRequiredService<EmailSettings>(); // juiste settings
//    var httpClient = sp.GetRequiredService<HttpClient>();
//    var logger = sp.GetRequiredService<ILogger<TransipMailingService>>();
//    return new TransipMailingService( httpClient, settings, logger );
//} );

// MailingService gets the same EmailSettings as TransipMailingService
//builder.Services.AddSingleton<MailingService>();

builder.Services.Configure<EmailSettings>( builder.Configuration.GetSection( "EmailSettings" ) );
builder.Services.AddSingleton( sp => sp.GetRequiredService<IOptions<EmailSettings>>().Value );
var testPass = builder.Configuration["EmailSettings:SmtpPass"];
Console.WriteLine( $"Loaded SMTP password from configuration: {testPass}" );

if ( builder.Environment.IsDevelopment() )
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Optional: log to console welke omgeving draait
Console.WriteLine( $"Environment: {builder.Environment.EnvironmentName}" );
Console.WriteLine( $"DB Connection: {builder.Configuration.GetConnectionString( "DefaultConnection" ) ?? "No connection found"}" );

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo( "nl-NL" );
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo( "nl-NL" );

// --- Service registrations ---
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();

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
builder.Services.AddScoped<StageService>();
builder.Services.AddScoped<StageTypeService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<VolunteerService>();
builder.Services.AddScoped<FieldMappingService>();
builder.Services.AddScoped<UserContextHelper>();
builder.Services.AddScoped<PlanningService>();
builder.Services.AddHttpContextAccessor();

// TransipMailingService gebruikt HttpClientFactory -> altijd via AddHttpClient()
builder.Services.AddHttpClient<TransipMailingService>();

// MailingService werkt met databasegegevens en mail -> scoped
builder.Services.AddScoped<MailingService>();

// Logging hoeft geen per-request state -> singleton is goed
builder.Services.AddSingleton<IMailingLogger, ConsoleMailingLogger>();

// ASP.NET Identity helpers
builder.Services.AddScoped<IEmailSender<ApplicationUser>, TransipEmailSender<ApplicationUser>>();
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, LegacyPasswordHasher>();


builder.Services.AddDbContext<ApplicationDbContext>( options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString( "DefaultConnection" ),
        ServerVersion.AutoDetect( builder.Configuration.GetConnectionString( "DefaultConnection" ) )
    ) );

builder.Services.AddAuthentication( CookieAuthenticationDefaults.AuthenticationScheme ).AddCookie();
builder.Services.AddScoped<CustomAuthenticationService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<ProtectedSessionStorage>();

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddAuthorizationCore();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddSyncfusionBlazor();
builder.Services.AddSingleton( typeof( ISyncfusionStringLocalizer ), typeof( SyncfusionLocalizer ) );

WebApplication app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage( "/_Host" );

app.Run();
