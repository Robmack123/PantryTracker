using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PantryTracker.Models;

public class UserProfile
{
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    public string IdentityUserId { get; set; }

    public IdentityUser IdentityUser { get; set; }

    public int? HouseholdId { get; set; }
    public Household Household { get; set; }

    public ICollection<PantryItem> PantryItems { get; set; }

    public UserProfile()
    {
        PantryItems = new List<PantryItem>();
    }
}