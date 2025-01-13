using System.Collections.Generic;

namespace PantryTracker.Models.DTOs;

public class UserProfileDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string IdentityUserId { get; set; } // Links to IdentityUser
    public int? HouseholdId { get; set; } // Nullable if user is not in a household
    public string HouseholdName { get; set; } // Simplified representation of the household
    public List<PantryItemDTO> PantryItems { get; set; } // Use a DTO for pantry items
}
