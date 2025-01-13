using System.ComponentModel.DataAnnotations;

namespace PantryTracker.Models;

public class PantryItemCategory
{
    public int Id { get; set; }

    [Required]
    public int PantryItemId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    // Navigation properties
    public PantryItem PantryItem { get; set; }
    public Category Category { get; set; }
}