using System.Text.Json.Serialization;
using dotenv.net; // Import the dotenv.net package
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using PantryTracker.Data;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Add Kestrel server configuration for both HTTPS and required Azure port (8080)
builder.WebHost.ConfigureKestrel(options =>
{
    // Keep HTTPS binding
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps(); // Ensure HTTPS is enabled
    });

    // 🔹 Add explicit binding to port 8080 (needed for Azure)
    options.ListenAnyIP(8080);
});

// Load environment variables from .env file
DotEnv.Load();

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Add HttpClient for external API calls
builder.Services.AddHttpClient();

// Swagger/OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔹 Configure authentication with cookie-based login
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "PantryTrackerLoginCookie";
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.HttpOnly = true;
        options.Cookie.MaxAge = TimeSpan.FromDays(7); // Cookie expires in a week
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(24); // Cookie expires in 24 hours
    });

// Configure IdentityCore for user authentication
builder.Services.AddIdentityCore<IdentityUser>(config =>
{
    config.Password.RequireDigit = false;
    config.Password.RequiredLength = 8;
    config.Password.RequireLowercase = false;
    config.Password.RequireNonAlphanumeric = false;
    config.Password.RequireUppercase = false;
    config.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<PantryTrackerDbContext>();

// Allow passing datetimes without time zone data
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// 🔹 Get database connection string from environment variables
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
builder.Services.AddNpgsql<PantryTrackerDbContext>(databaseUrl);

var app = builder.Build();

// 🔹 Ensure HTTPS redirection
app.UseHttpsRedirection();

// 🔹 Enable Swagger in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 Ensure authentication & authorization are correctly applied
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Map controllers
app.MapControllers();

// 🔹 Start the application
app.Run();
