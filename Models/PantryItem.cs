using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PantryTracker.Models
{
    public class PantryItem
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Quantity { get; set; }

        public DateTime UpdatedAt { get; set; }

        [Required]
        public int HouseholdId { get; set; }
        public bool MonitorLowStock { get; set; } = true;
        public int LowStockThreshold { get; set; }
        // Navigation properties
        public Household Household { get; set; }
        public ICollection<Category> Categories { get; set; }

        public PantryItem()
        {
            Categories = new List<Category>();
        }
    }
}
