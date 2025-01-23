public class UpdatePantryItemDTO
{
    public string Name { get; set; } // Allow updating name
    public int? LowStockThreshold { get; set; } // Allow updating low stock threshold
    public int? Quantity { get; set; } // Allow updating quantity
}
