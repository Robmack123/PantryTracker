using System.ComponentModel.DataAnnotations;

namespace PantryTracker.Models;
public class Household
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public DateTime CreatedAt { get; set; }

    [Required]
    public string JoinCode { get; set; }

    public ICollection<UserProfile> Users { get; set; }
    public ICollection<PantryItem> PantryItems { get; set; }

    public Household()
    {
        Users = new List<UserProfile>();
        PantryItems = new List<PantryItem>();
    }
}