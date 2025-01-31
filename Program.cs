using System.Text.Json.Serialization;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using PantryTracker.Data;

var builder = WebApplication.CreateBuilder(args);

// Logging to help diagnose startup failures
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configure Kestrel to listen on both 8080 and 8181 for Azure
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
    options.ListenAnyIP(8181);
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

// Configure authentication with cookie-based login
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "PantryTrackerLoginCookie";
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.HttpOnly = true;
        options.Cookie.MaxAge = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
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

// Get database connection string from environment variables
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
        Console.WriteLine("Database connection initialized successfully.");
        builder.Services.AddNpgsql<PantryTrackerDbContext>(databaseUrl);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to connect to the database: {ex.Message}");
    throw;
}

// Configure CORS to allow requests from Amplify (frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder.WithOrigins("https://deployment.d1n47r1bcwr1gk.amplifyapp.com")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

var app = builder.Build();

// Disable HTTPS redirection in production (Azure handles it)
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Enable Swagger in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure authentication & authorization are correctly applied
app.UseAuthentication();
app.UseAuthorization();

// Enable CORS policy for the app
app.UseCors("AllowFrontend");

// Health check endpoint for debugging
app.MapGet("/health", () => Results.Ok("Healthy"));

// Log app startup success
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("PantryTracker API has started successfully!");
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
