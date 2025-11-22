using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Debug: dump all config providers
var root = (IConfigurationRoot)builder.Configuration;

foreach (var provider in root.Providers)
{
    Console.WriteLine("Provider: " + provider);
}

// -------------------------------
// 1. Load configuration in a strict order
// -------------------------------
// NOTE: This ensures Development overrides are always applied.
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // Highest priority

// -------------------------------
// 2. Retrieve connectionstring (sanity check)
// -------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Dump na laden
Console.WriteLine("=== CONFIG DUMP ===");
Console.WriteLine("ENV: " + builder.Environment.EnvironmentName);
Console.WriteLine("ConnectionString: " + builder.Configuration.GetConnectionString("DefaultConnection"));
Console.WriteLine("====================");

// Debug output
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"DB Connection: {(string.IsNullOrWhiteSpace(connectionString) ? "NOT FOUND" : "FOUND")}");

// -------------------------------
// 3. Register services
// -------------------------------
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Example: register your Database layer
// builder.Services.AddSingleton<MyDatabaseService>(new MyDatabaseService(connectionString));

// -------------------------------
// 4. Build app
// -------------------------------
var app = builder.Build();

// -------------------------------
// 5. Middleware pipeline
// -------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // Optional debugging helpers
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
