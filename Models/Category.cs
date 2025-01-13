using System.ComponentModel.DataAnnotations;
using PantryTracker.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    // Navigation property
    public ICollection<PantryItem> PantryItems { get; set; }
    public Category()
    {
        PantryItems = new List<PantryItem>();
    }
}
