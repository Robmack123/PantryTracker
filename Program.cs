using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using dotenv.net;
using Microsoft.AspNetCore.Identity;
using PantryTracker.Data;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file **before building services**
DotEnv.Load();

// Add Controllers and JSON serialization settings
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Add HttpClient for external API calls
builder.Services.AddHttpClient();
builder.Services.AddHttpClient();

// Swagger/OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure authentication with cookie‑based login
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "PantryTrackerLoginCookie";
        options.Cookie.SameSite = SameSiteMode.Strict; // Use Strict (or adjust if needed)
        options.Cookie.HttpOnly = true;
        options.Cookie.MaxAge = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(24);

        // Prevent redirects for API calls (return 401 instead)
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
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

// Get the database connection string from environment variables
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrEmpty(databaseUrl))
{
    Console.WriteLine("ERROR: DATABASE_URL is missing in Azure. Ensure it is set in Application Settings.");
    throw new InvalidOperationException("DATABASE_URL environment variable is missing.");
}
else
{
    var maskedDatabaseUrl = System.Text.RegularExpressions.Regex.Replace(databaseUrl, @"Password=[^;]*", "Password=****");
    Console.WriteLine($"DATABASE_URL is loaded: {maskedDatabaseUrl}");
}

// Initialize PostgreSQL database connection
builder.Services.AddNpgsql<PantryTrackerDbContext>(databaseUrl);
Console.WriteLine("Database connection initialized successfully.");

// Configure CORS to allow requests from Amplify (frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policyBuilder =>
    {
        policyBuilder.WithOrigins("https://deployment.d1n47r1bcwr1gk.amplifyapp.com")
                     .AllowAnyMethod()
                     .AllowAnyHeader()
                     .AllowCredentials();
    });
});

// Database configuration
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrEmpty(databaseUrl))
{
    throw new InvalidOperationException("DATABASE_URL environment variable is missing.");
}
builder.Services.AddNpgsql<PantryTrackerDbContext>(databaseUrl);

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint for debugging
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("PantryTracker API has started successfully!");
});

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
