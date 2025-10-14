using System.Globalization;

using Amusing.Components.Account;
using Amusing.Data;
using Amusing.Security;
using Amusing.Services;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;

using Syncfusion.Blazor;

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense( "Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZfeHRRR2ZeUEVyX0FWYEg=" );

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo( "nl-NL" );
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo( "nl-NL" );

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GenericDataService>();
builder.Services.AddScoped<CountryService>();
builder.Services.AddScoped<EditionService>();
builder.Services.AddScoped<EmailAddressesService>();
builder.Services.AddScoped<FestivalService>();
builder.Services.AddScoped<GenreService>();
builder.Services.AddScoped<GitHubService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<MailingService>();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddScoped<StageService>();
builder.Services.AddScoped<StageTypeService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<VolunteerService>();
builder.Services.AddHttpClient<TransipMailingService>();
builder.Services.AddScoped<IEmailSender<ApplicationUser>, TransipEmailSender<ApplicationUser>>();
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, LegacyPasswordHasher>();

builder.Services.AddDbContext<ApplicationDbContext>( options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString( "DefaultConnection" ),
        ServerVersion.AutoDetect( builder.Configuration.GetConnectionString( "DefaultConnection" ) )
    ) );

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication( CookieAuthenticationDefaults.AuthenticationScheme ).AddCookie();
builder.Services.AddScoped<CustomAuthenticationService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<ProtectedSessionStorage>();
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
