using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PantryTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace PantryTracker.Data;
public class PantryTrackerDbContext : IdentityDbContext<IdentityUser>
{
    private readonly IConfiguration _configuration;
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Household> Households { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<PantryItem> PantryItems { get; set; }

    public PantryTrackerDbContext(DbContextOptions<PantryTrackerDbContext> context, IConfiguration config) : base(context)
    {
        _configuration = config;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Household>()
            .HasOne(h => h.AdminUser)
            .WithMany()
            .HasForeignKey(h => h.AdminUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PantryItem>()
            .HasMany(pi => pi.Categories)
            .WithMany(c => c.PantryItems);

        modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole
        {
            Id = "c3aaeb97-d2ba-4a53-a521-4eea61e59b35",
            Name = "Admin",
            NormalizedName = "admin"
        });

        // Ensure admin password is loaded from configuration
        var adminPassword = _configuration["AdminPassword"];
        if (string.IsNullOrEmpty(adminPassword))
        {
            throw new InvalidOperationException("Admin password is not set in the configuration.");
        }

        var passwordHasher = new PasswordHasher<IdentityUser>();
        var hashedPassword = passwordHasher.HashPassword(null, adminPassword);

        modelBuilder.Entity<IdentityUser>().HasData(new IdentityUser
        {
            Id = "dbc40bc6-0829-4ac5-a3ed-180f5e916a5f",
            UserName = "Administrator",
            Email = "admina@strator.comx",
            PasswordHash = hashedPassword
        });

        modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
        {
            RoleId = "c3aaeb97-d2ba-4a53-a521-4eea61e59b35",
            UserId = "dbc40bc6-0829-4ac5-a3ed-180f5e916a5f"
        });

        modelBuilder.Entity<UserProfile>().HasData(new UserProfile
        {
            Id = 1,
            IdentityUserId = "dbc40bc6-0829-4ac5-a3ed-180f5e916a5f",
            FirstName = "Admina",
            LastName = "Strator",
        });

        modelBuilder.Entity<Household>().HasData(new Household
        {
            Id = 1,
            Name = "Admin Household",
            CreatedAt = DateTime.UtcNow,
            JoinCode = "ADMIN123",
            AdminUserId = 1
        });

        // Adding all categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Dairy" },
            new Category { Id = 2, Name = "Snacks" },
            new Category { Id = 3, Name = "Beverages" },
            new Category { Id = 4, Name = "Produce" },
            new Category { Id = 5, Name = "Canned Goods" },
            new Category { Id = 6, Name = "Condiments & Sauces" },
            new Category { Id = 7, Name = "Dry Goods" },
            new Category { Id = 8, Name = "Proteins" },
            new Category { Id = 9, Name = "Baking Supplies" },
            new Category { Id = 10, Name = "Breakfast Foods" },
            new Category { Id = 11, Name = "Frozen Foods" },
            new Category { Id = 12, Name = "Spices & Seasonings" },
            new Category { Id = 13, Name = "Oils & Fats" },
            new Category { Id = 14, Name = "Prepared Foods" }
        );

        modelBuilder.Entity<PantryItem>().HasData(
            new PantryItem
            {
                Id = 1,
                Name = "Milk",
                Quantity = 2,
                UpdatedAt = DateTime.UtcNow,
                HouseholdId = 1,
            },
            new PantryItem
            {
                Id = 2,
                Name = "Cheese",
                Quantity = 5,
                UpdatedAt = DateTime.UtcNow,
                HouseholdId = 1,
            },
            new PantryItem
            {
                Id = 3,
                Name = "Bread",
                Quantity = 3,
                UpdatedAt = DateTime.UtcNow,
                HouseholdId = 1,
            }
        );

        // Mapping pantry items to categories
        modelBuilder.Entity<PantryItem>()
            .HasMany(pi => pi.Categories)
            .WithMany(c => c.PantryItems)
            .UsingEntity(j => j.HasData(
                new { PantryItemsId = 1, CategoriesId = 1 }, // Milk -> Dairy
                new { PantryItemsId = 2, CategoriesId = 1 }, // Cheese -> Dairy
                new { PantryItemsId = 3, CategoriesId = 4 }  // Bread -> Produce
        ));
    }
}
