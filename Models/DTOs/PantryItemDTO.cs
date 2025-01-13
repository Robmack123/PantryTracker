namespace PantryTracker.Models.DTOs;

public class PantryItemDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public DateTime UpdatedAt { get; set; }
}
