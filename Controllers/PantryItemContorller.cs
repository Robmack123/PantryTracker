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
        public IActionResult GetPantryItems()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userProfile = _dbContext.UserProfiles
                    .FirstOrDefault(up => up.IdentityUserId == userId);

                if (userProfile == null || userProfile.HouseholdId == null)
                {
                    Console.WriteLine("User is not part of a household.");
                    return BadRequest(new { Message = "You are not part of a household." });
                }

                var pantryItems = _dbContext.PantryItems
                    .Where(pi => pi.HouseholdId == userProfile.HouseholdId)
                    .Select(pi => new PantryItemDTO
                    {
                        Id = pi.Id,
                        Name = pi.Name,
                        Quantity = pi.Quantity,
                        UpdatedAt = pi.UpdatedAt
                    })
                    .ToList();

                Console.WriteLine($"Pantry items fetched: {pantryItems.Count}");
                return Ok(pantryItems);
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
                    .Include(pi => pi.Categories) // Eagerly load Categories
                    .FirstOrDefault(pi =>
                        pi.Name.ToLower() == pantryItemDto.Name.ToLower() &&
                        pi.HouseholdId == userProfile.HouseholdId);

                if (existingItem != null)
                {
                    // If it exists, increase the quantity and update the timestamp
                    existingItem.Quantity += pantryItemDto.Quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;

                    // Update categories if provided
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

                    // Update the existing item
                    _dbContext.PantryItems.Update(existingItem);
                }
                else
                {
                    // If it doesn't exist, create a new item
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

                // Save changes once at the end
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

    }
}