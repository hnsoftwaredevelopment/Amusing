using Amusing.Services;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();

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

WebApplication app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage( "/_Host" );

app.Run();
