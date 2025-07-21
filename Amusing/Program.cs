using System.Globalization;

using Amusing.Services;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

using Syncfusion.Blazor;

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense( "Mgo+DSMBPh8sVXN0S0d+X1ZPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9mSXlSdkVgWHpfdXBVQmNXUkQ=;Mgo+DSMBMAY9C3t3VVhhQlJDfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTH5Ud0VjWn5bcXFRR2lVWkd2;Mzk0NDI0MUAzMzMwMmUzMDJlMzAzYjMzMzAzYk1jRWttUUNkT0x3SGtCeTlNQUNKWlA4dEtPcHpPUG9DUGxTUXJLMGtPc0U9" );

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo( "nl-NL" );
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo( "nl-NL" );

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GenericDataService>();
builder.Services.AddScoped<EditionService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddScoped<VolunteerService>();
builder.Services.AddScoped<EmailAddressesService>();

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

WebApplication app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage( "/_Host" );

app.Run();
