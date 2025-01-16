using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryTracker.Data;
using PantryTracker.Models;
using PantryTracker.Models.DTOs;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;


namespace PantryTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PantryItemController : ControllerBase
    {
        private readonly PantryTrackerDbContext _dbContext;

        public PantryItemController(PantryTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetPantryItems(int page = 1, int pageSize = 10, string searchQuery = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userProfile = _dbContext.UserProfiles
                    .FirstOrDefault(up => up.IdentityUserId == userId);

                if (userProfile == null || userProfile.HouseholdId == null)
                {
                    return BadRequest(new { Message = "You are not part of a household." });
                }

                var query = _dbContext.PantryItems
                    .Where(pi => pi.HouseholdId == userProfile.HouseholdId);

                // Apply search query if provided (case-insensitive)
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query = query.Where(pi => pi.Name.ToLower().Contains(searchQuery.ToLower()));
                }

                var pantryItems = query
                    .OrderBy(pi => pi.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(pi => new PantryItemDTO
                    {
                        Id = pi.Id,
                        Name = pi.Name,
                        Quantity = pi.Quantity,
                        UpdatedAt = pi.UpdatedAt
                    })
                    .ToList();

                var totalItems = query.Count();

                return Ok(new
                {
                    Items = pantryItems,
                    TotalItems = totalItems
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching pantry items: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred.", Exception = ex.Message });
            }
        }





        [HttpPost]
        [Authorize]
        public IActionResult AddOrUpdatePantryItem([FromBody] PantryItemDTO pantryItemDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userProfile = _dbContext.UserProfiles
                    .FirstOrDefault(up => up.IdentityUserId == userId);

                if (userProfile == null || userProfile.HouseholdId == null)
                {
                    return BadRequest(new { Message = "You are not part of a household." });
                }

                // Check if the item already exists in the household's pantry
                var existingItem = _dbContext.PantryItems
                    .Include(pi => pi.Categories)
                    .FirstOrDefault(pi =>
                        pi.Name.ToLower() == pantryItemDto.Name.ToLower() &&
                        pi.HouseholdId == userProfile.HouseholdId);

                if (existingItem != null)
                {
                    // If it exists, increase the quantity and update the timestamp
                    existingItem.Quantity += pantryItemDto.Quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;

                    if (pantryItemDto.CategoryIds != null && pantryItemDto.CategoryIds.Count > 0)
                    {
                        var categories = _dbContext.Categories
                            .Where(c => pantryItemDto.CategoryIds.Contains(c.Id))
                            .ToList();

                        foreach (var category in categories)
                        {
                            if (!existingItem.Categories.Any(c => c.Id == category.Id))
                            {
                                existingItem.Categories.Add(category);
                            }
                        }
                    }

                    _dbContext.PantryItems.Update(existingItem);
                }
                else
                {
                    var newPantryItem = new PantryItem
                    {
                        Name = pantryItemDto.Name,
                        Quantity = pantryItemDto.Quantity,
                        UpdatedAt = DateTime.UtcNow,
                        HouseholdId = userProfile.HouseholdId.Value,
                        Categories = _dbContext.Categories
                            .Where(c => pantryItemDto.CategoryIds.Contains(c.Id))
                            .ToList() // Add relationships during creation
                    };

                    _dbContext.PantryItems.Add(newPantryItem);
                }

                _dbContext.SaveChanges();

                return Ok(new { Message = "Pantry item successfully added or updated." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding or updating pantry item: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return StatusCode(500, new
                {
                    Message = "An error occurred.",
                    Exception = ex.Message,
                    InnerException = ex.InnerException?.Message
                });
            }
        }


        [HttpGet("by-category")]
        [Authorize]
        public IActionResult GetPantryItemsByCategory([FromQuery] List<int> categoryIds)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userProfile = _dbContext.UserProfiles
                    .FirstOrDefault(up => up.IdentityUserId == userId);

                if (userProfile == null || userProfile.HouseholdId == null)
                {
                    return BadRequest(new { Message = "You are not part of a household." });
                }

                // Fetch pantry items filtered by category IDs
                var pantryItems = _dbContext.PantryItems
                    .Where(pi => pi.HouseholdId == userProfile.HouseholdId)
                    .Where(pi => pi.Categories.Any(c => categoryIds.Contains(c.Id)))
                    .Select(pi => new PantryItemDTO
                    {
                        Id = pi.Id,
                        Name = pi.Name,
                        Quantity = pi.Quantity,
                        UpdatedAt = pi.UpdatedAt,
                        CategoryIds = pi.Categories.Select(c => c.Id).ToList()
                    })
                    .ToList();

                return Ok(pantryItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching pantry items by category: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred.", Exception = ex.Message });
            }
        }

        [HttpPut("{id}/quantity")]
        [Authorize]
        public IActionResult UpdatePantryItemQuantity(int id, [FromBody] UpdatePantryItemQuantityDTO dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userProfile = _dbContext.UserProfiles
                    .FirstOrDefault(up => up.IdentityUserId == userId);

                if (userProfile == null || userProfile.HouseholdId == null)
                {
                    return BadRequest(new { Message = "You are not part of a household." });
                }

                if (dto.Quantity < 0)
                {
                    return BadRequest(new { Message = "Quantity cannot be negative." });
                }

                var pantryItem = _dbContext.PantryItems
                    .FirstOrDefault(pi => pi.Id == id && pi.HouseholdId == userProfile.HouseholdId);

                if (pantryItem == null)
                {
                    return NotFound(new { Message = "Pantry item not found or does not belong to your household." });
                }

                pantryItem.Quantity = dto.Quantity;
                pantryItem.UpdatedAt = DateTime.UtcNow;

                _dbContext.SaveChanges();

                return Ok(new { Message = "Pantry item quantity updated successfully.", PantryItem = pantryItem });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating pantry item quantity: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred.", Exception = ex.Message });
            }
        }
        [HttpGet("recent-activity")]
        [Authorize]
        public IActionResult GetRecentActivity()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userProfile = _dbContext.UserProfiles
                    .FirstOrDefault(up => up.IdentityUserId == userId);

                if (userProfile == null || userProfile.HouseholdId == null)
                {
                    return BadRequest(new { Message = "You are not part of a household." });
                }

                // Fetch recent activity (e.g., added/updated items in the last 7 days)
                var recentItems = _dbContext.PantryItems
                    .Where(pi => pi.HouseholdId == userProfile.HouseholdId)
                    .OrderByDescending(pi => pi.UpdatedAt)
                    .Take(10) // Limit to the most recent 10 items
                    .Select(pi => new PantryItemDTO
                    {
                        Id = pi.Id,
                        Name = pi.Name,
                        Quantity = pi.Quantity,
                        UpdatedAt = pi.UpdatedAt
                    })
                    .ToList();

                // Fetch low-stock items
                var lowStockItems = _dbContext.PantryItems
                    .Where(pi => pi.HouseholdId == userProfile.HouseholdId && pi.Quantity < 2)
                    .Select(pi => new PantryItemDTO
                    {
                        Id = pi.Id,
                        Name = pi.Name,
                        Quantity = pi.Quantity,
                        UpdatedAt = pi.UpdatedAt
                    })
                    .ToList();

                return Ok(new
                {
                    RecentActivity = recentItems,
                    LowStockItems = lowStockItems
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching recent activity: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred.", Exception = ex.Message });
            }
        }

    }
}