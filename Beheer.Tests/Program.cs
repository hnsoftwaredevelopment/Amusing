using System.Globalization;

using Amusing.Services;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

using Syncfusion.Blazor;

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense( "Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZfeHRRR2ZeUEVyX0FWYEg=" );

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("nl-NL");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("nl-NL");

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
builder.Services.AddScoped<VolunteerService>();

builder.Services.AddAuthentication( CookieAuthenticationDefaults.AuthenticationScheme )
    .AddCookie( options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
    } );

builder.Services.AddAuthorization();

// Register de bestaande authenticatieservice
builder.Services.AddScoped<CustomAuthenticationService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

//builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSyncfusionBlazor();
builder.Services.AddSingleton(typeof(ISyncfusionStringLocalizer), typeof(SyncfusionLocalizer));

WebApplication app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage( "/_Host" );

app.Run();
