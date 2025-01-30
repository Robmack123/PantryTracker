using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;  // Ensure this namespace is included
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
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables() // Load environment variables
                .Build();

            // Get the database connection string from environment variables
            var databaseUrl = configuration["DATABASE_URL"];

            if (string.IsNullOrEmpty(databaseUrl))
            {
                throw new InvalidOperationException("DATABASE_URL environment variable is not set.");
            }

            // Configure the DbContext with the connection string
            optionsBuilder.UseNpgsql(databaseUrl);

            return new PantryTrackerDbContext(optionsBuilder.Options, configuration);
        }
    }
}
