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

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Make sure to use the correct case for the password
            var adminPassword = configuration["ADMIN_PASSWORD"];
            if (string.IsNullOrEmpty(adminPassword))
            {
                throw new InvalidOperationException("Admin password is not set in the configuration.");
            }

            var databaseUrl = configuration["DATABASE_URL"];
            if (string.IsNullOrEmpty(databaseUrl))
            {
                throw new InvalidOperationException("DATABASE_URL environment variable is not set.");
            }

            optionsBuilder.UseNpgsql(databaseUrl);

            return new PantryTrackerDbContext(optionsBuilder.Options, configuration);
        }

    }
}
