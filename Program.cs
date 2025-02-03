using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using dotenv.net;
using Microsoft.AspNetCore.Identity;
using PantryTracker.Data;
using System.Text.Json.Serialization;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


var builder = WebApplication.CreateBuilder(args);

// Logging to help diagnose startup failures
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Load environment variables from .env file **before building services**
DotEnv.Load();

// Add Controllers and JSON serialization settings
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// (Other services like HttpClient, Swagger, etc.)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT Authentication
// (Replace "YOUR_SECRET_KEY_HERE" with your own secret key. You may want to load it from configuration.)
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "YOUR_SECRET_KEY_HERE";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "YourIssuer";
var audience = builder.Configuration["Jwt:Audience"] ?? "YourAudience";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
    };
});

// Configure IdentityCore for user authentication (remains mostly unchanged)
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

// (Remaining code to set up DB connection, CORS, etc. remains the same)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
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
try
{
    builder.Services.AddNpgsql<PantryTrackerDbContext>(databaseUrl);
    Console.WriteLine("Database connection initialized successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to connect to the database: {ex.Message}");
    throw;
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policyBuilder =>
        {
            policyBuilder.WithOrigins("https://deployment.d1n47r1bcwr1gk.amplifyapp.com")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// IMPORTANT: Use CORS and authentication middleware
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
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
