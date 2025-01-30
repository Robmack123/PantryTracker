using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PantryTracker.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PantryTrackerDbContext>
    {
        public PantryTrackerDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PantryTrackerDbContext>();

            // Set up configuration to load environment variables
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // This points to your project directory
                .AddJsonFile("appsettings.json", optional: true) // Optional if you want appsettings as well
                .AddEnvironmentVariables() // Ensure environment variables are loaded from .env
                .Build();

            // Check if the AdminPassword is available
            var adminPassword = configuration["ADMIN_PASSWORD"];
            if (string.IsNullOrEmpty(adminPassword))
            {
                throw new InvalidOperationException("Admin password is not set in the configuration.");
            }

            // Log the password for debugging (remove this after confirming it's working)
            Console.WriteLine($"Loaded Admin Password: {adminPassword}");

            var databaseUrl = configuration["DATABASE_URL"];
            if (string.IsNullOrEmpty(databaseUrl))
            {
                throw new InvalidOperationException("DATABASE_URL environment variable is not set.");
            }

            // Configure DbContext with the connection string
            optionsBuilder.UseNpgsql(databaseUrl);

            return new PantryTrackerDbContext(optionsBuilder.Options, configuration);
        }
    }
}
