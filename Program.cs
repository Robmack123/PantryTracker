using System.Text.Json.Serialization;
using dotenv.net; // Import the dotenv.net package
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using PantryTracker.Data;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
DotEnv.Load(); // Ensure this is called first to load environment variables

// Retrieve environment variables
var chompApiKey = Environment.GetEnvironmentVariable("CHOMP_API_KEY");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Add HttpClient for external API calls
builder.Services.AddHttpClient();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "PantryTrackerLoginCookie";
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.HttpOnly = true; // The cookie cannot be accessed through JS (protects against XSS)
        options.Cookie.MaxAge = new TimeSpan(7, 0, 0, 0); // Cookie expires in a week regardless of activity
        options.SlidingExpiration = true; // Extend the cookie lifetime with activity up to 7 days
        options.ExpireTimeSpan = new TimeSpan(24, 0, 0); // Cookie will expire in 24 hours without activity
        options.Events.OnRedirectToLogin = (context) =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = (context) =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddIdentityCore<IdentityUser>(config =>
    {
        // Adjust password requirements for production if needed
        config.Password.RequireDigit = false;
        config.Password.RequiredLength = 8;
        config.Password.RequireLowercase = false;
        config.Password.RequireNonAlphanumeric = false;
        config.Password.RequireUppercase = false;
        config.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()  // Add the role service  
    .AddEntityFrameworkStores<PantryTrackerDbContext>();

// Allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Configure database connection using environment variable
if (string.IsNullOrEmpty(databaseUrl))
{
    throw new InvalidOperationException("DATABASE_URL environment variable is not set.");
}

builder.Services.AddNpgsql<PantryTrackerDbContext>(databaseUrl);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// These two calls are required to add auth to the pipeline for a request
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
