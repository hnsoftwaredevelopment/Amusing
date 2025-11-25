using System.Diagnostics;
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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Syncfusion.Blazor;

// Register Syncfusion
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
    "Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZfeHRRR2ZeUEVyX0FWYEg="
);

var builder = WebApplication.CreateBuilder(args);

// Debug: dump all config providers
var root = (IConfigurationRoot)builder.Configuration;
foreach ( var provider in root.Providers )
{
    Debug.WriteLine( "Provider: " + provider );
}

// ---------------------------------------------------------
// 1. Configuration loading (correct load order)
// ---------------------------------------------------------
builder.Configuration
    .SetBasePath( Directory.GetCurrentDirectory() )
    .AddJsonFile( "appsettings.json", optional: false, reloadOnChange: true )
    .AddJsonFile( $"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true )
    .AddUserSecrets<Program>( optional: true )
    .AddEnvironmentVariables();

// -------------------------------
// 2. Retrieve connectionstring (sanity check)
// -------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");



// Dump na laden
Debug.WriteLine( "=== CONFIG DUMP ===" );
Debug.WriteLine( "ENV: " + builder.Environment.EnvironmentName );
Debug.WriteLine( "ConnectionString: " + builder.Configuration.GetConnectionString( "DefaultConnection" ) );
Debug.WriteLine( "====================" );

// Debug output
Debug.WriteLine( $"Environment: {builder.Environment.EnvironmentName}" );
Debug.WriteLine( $"DB Connection: {( string.IsNullOrWhiteSpace( connectionString ) ? "NOT FOUND" : "FOUND" )}" );

// ---------------------------------------------------------
// 2. Email settings configuration
// ---------------------------------------------------------
builder.Services.Configure<EmailSettings>( builder.Configuration.GetSection( "EmailSettings" ) );
builder.Services.AddSingleton( sp => sp.GetRequiredService<IOptions<EmailSettings>>().Value );

var smtpPass = builder.Configuration["EmailSettings:SmtpPass"];
Console.WriteLine( $"SMTP Password Loaded: {smtpPass ?? "NOT FOUND"}" );

// ---------------------------------------------------------
// 3. Culture settings
// ---------------------------------------------------------
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo( "nl-NL" );
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo( "nl-NL" );

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
builder.Services.AddScoped<StageService>();
builder.Services.AddScoped<StageTypeService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<VolunteerService>();
builder.Services.AddScoped<FieldMappingService>();
builder.Services.AddScoped<UserContextHelper>();
builder.Services.AddScoped<PlanningService>();

builder.Services.AddBlazoredSessionStorage();

// Email + HTTP related
builder.Services.AddHttpClient<TransipMailingService>();
builder.Services.AddScoped<MailingService>();
builder.Services.AddSingleton<IMailingLogger, ConsoleMailingLogger>();

// Identity
builder.Services.AddScoped<IEmailSender<ApplicationUser>, TransipEmailSender<ApplicationUser>>();
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, LegacyPasswordHasher>();

builder.Services.AddAuthentication( CookieAuthenticationDefaults.AuthenticationScheme )
    .AddCookie();

builder.Services.AddScoped<CustomAuthenticationService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<ProtectedSessionStorage>();

builder.Services.AddAuthorizationCore();

// ---------------------------------------------------------
// 5. Database configuration
// ---------------------------------------------------------
var conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>( options =>
    options.UseMySql( conn, ServerVersion.AutoDetect( conn ) )
);

// ---------------------------------------------------------
// 6. Syncfusion
// ---------------------------------------------------------
builder.Services.AddSyncfusionBlazor();
builder.Services.AddSingleton( typeof( ISyncfusionStringLocalizer ), typeof( SyncfusionLocalizer ) );

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// ---------------------------------------------------------
// Build & middleware pipeline
// ---------------------------------------------------------
var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage( "/_Host" );

app.Run();
