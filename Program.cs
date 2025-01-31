using System.Text.Json.Serialization;
using dotenv.net; // Import the dotenv.net package
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using PantryTracker.Data;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Add logging to help diagnose startup failures
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// 🔹 Add Kestrel server configuration to ensure it listens on port 8080 for Azure
builder.WebHost.ConfigureKestrel(options =>
{
    // Ensure the app listens only on port 8080 (required for Azure)
    options.ListenAnyIP(8080);
});

// Load environment variables from .env file **before building services**
DotEnv.Load();

// Add Controllers and JSON serialization settings
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Add HttpClient for external API calls
try
{
    builder.Services.AddHttpClient();
    Console.WriteLine("HttpClient service configured.");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to configure HttpClient: {ex.Message}");
    throw;
}

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
try
{
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

    Console.WriteLine("Identity services configured.");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to configure Identity: {ex.Message}");
    throw;
}

// Allow passing datetimes without time zone data
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// 🔹 Get database connection string from environment variables
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

try
{
    if (string.IsNullOrEmpty(databaseUrl))
    {
        Console.WriteLine("DATABASE_URL is missing in Azure.");
        throw new InvalidOperationException("DATABASE_URL environment variable is missing.");
    }
    else
    {
        Console.WriteLine($"DATABASE_URL is: {databaseUrl}");
        builder.Services.AddNpgsql<PantryTrackerDbContext>(databaseUrl);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to connect to the database: {ex.Message}");
    throw;
}

// 🔹 Configure CORS to allow requests from Amplify (frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

// 🔹 Ensure HTTPS redirection (this will be handled by Azure)
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

// 🔹 Enable CORS policy for the app
app.UseCors("AllowFrontend");

// 🔹 Log app startup success
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("🚀 PantryTracker API has started successfully!");
});

app.MapControllers();

// Start the app
try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"FATAL ERROR during app run: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    throw;
}
