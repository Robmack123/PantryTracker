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

        modelBuilder.Entity<IdentityUser>().HasData(new IdentityUser
        {
            Id = "dbc40bc6-0829-4ac5-a3ed-180f5e916a5f",
            UserName = "Administrator",
            Email = "admina@strator.comx",
            PasswordHash = new PasswordHasher<IdentityUser>().HashPassword(null, _configuration["AdminPassword"])
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
        modelBuilder.Entity<Category>().HasData(
           new Category { Id = 1, Name = "Dairy" },
           new Category { Id = 2, Name = "Snacks" },
           new Category { Id = 3, Name = "Beverages" },
           new Category { Id = 4, Name = "Produce" }
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