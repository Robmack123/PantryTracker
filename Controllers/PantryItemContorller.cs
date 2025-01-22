using System.Net.Http;
using System.Text.Json;
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
        private readonly HttpClient _httpClient;

        public PantryItemController(PantryTrackerDbContext dbContext, HttpClient httpClient)
        {
            _dbContext = dbContext;
            _httpClient = httpClient; // Injected HttpClient
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
                        UpdatedAt = pi.UpdatedAt,
                        MonitorLowStock = pi.MonitorLowStock
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

                var existingItem = _dbContext.PantryItems
                    .Include(pi => pi.Categories)
                    .FirstOrDefault(pi =>
                        pi.Name.ToLower() == pantryItemDto.Name.ToLower() &&
                        pi.HouseholdId == userProfile.HouseholdId);

                if (existingItem != null)
                {
                    existingItem.Quantity += pantryItemDto.Quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    existingItem.MonitorLowStock = pantryItemDto.MonitorLowStock; // Update MonitorLowStock

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
                        MonitorLowStock = pantryItemDto.MonitorLowStock,
                        Categories = _dbContext.Categories
                            .Where(c => pantryItemDto.CategoryIds.Contains(c.Id))
                            .ToList()
                    };

                    _dbContext.PantryItems.Add(newPantryItem);
                }

                _dbContext.SaveChanges();

                return Ok(new { Message = "Pantry item successfully added or updated." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding or updating pantry item: {ex.Message}");
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

                var pantryItems = _dbContext.PantryItems
                    .Where(pi => pi.HouseholdId == userProfile.HouseholdId)
                    .Where(pi => pi.Categories.Any(c => categoryIds.Contains(c.Id)))
                    .Select(pi => new PantryItemDTO
                    {
                        Id = pi.Id,
                        Name = pi.Name,
                        Quantity = pi.Quantity,
                        UpdatedAt = pi.UpdatedAt,
                        CategoryIds = pi.Categories.Select(c => c.Id).ToList(),
                        MonitorLowStock = pi.MonitorLowStock
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

                var recentItems = _dbContext.PantryItems
                    .Where(pi => pi.HouseholdId == userProfile.HouseholdId)
                    .OrderByDescending(pi => pi.UpdatedAt)
                    .Take(10)
                    .Select(pi => new PantryItemDTO
                    {
                        Id = pi.Id,
                        Name = pi.Name,
                        Quantity = pi.Quantity,
                        UpdatedAt = pi.UpdatedAt,
                        MonitorLowStock = pi.MonitorLowStock
                    })
                    .ToList();

                var lowStockItems = _dbContext.PantryItems
                    .Where(pi => pi.HouseholdId == userProfile.HouseholdId && pi.Quantity < 3 && pi.MonitorLowStock)
                    .Select(pi => new PantryItemDTO
                    {
                        Id = pi.Id,
                        Name = pi.Name,
                        Quantity = pi.Quantity,
                        UpdatedAt = pi.UpdatedAt,
                        MonitorLowStock = pi.MonitorLowStock
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

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeletePantryItem(int id)
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

                var pantryItem = _dbContext.PantryItems
                    .FirstOrDefault(pi => pi.Id == id && pi.HouseholdId == userProfile.HouseholdId);

                if (pantryItem == null)
                {
                    return NotFound(new { Message = "Pantry item not found or does not belong to your household." });
                }

                _dbContext.PantryItems.Remove(pantryItem);
                _dbContext.SaveChanges();

                return Ok(new { Message = "Pantry item deleted successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting pantry item: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred.", Exception = ex.Message });
            }
        }

        [HttpPut("{id}/toggle-monitor")]
        [Authorize]
        public IActionResult ToggleMonitorLowStock(int id)
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

                var pantryItem = _dbContext.PantryItems
                    .FirstOrDefault(pi => pi.Id == id && pi.HouseholdId == userProfile.HouseholdId);

                if (pantryItem == null)
                {
                    return NotFound(new { Message = "Pantry item not found or does not belong to your household." });
                }

                pantryItem.MonitorLowStock = !pantryItem.MonitorLowStock;
                _dbContext.SaveChanges();

                return Ok(new { Message = "MonitorLowStock toggled successfully.", MonitorLowStock = pantryItem.MonitorLowStock });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling MonitorLowStock: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred.", Exception = ex.Message });
            }
        }

        [HttpGet("search-branded")]
        [Authorize]
        public async Task<IActionResult> SearchBrandedFood(string name, int limit = 10, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { Message = "Food name cannot be empty." });
            }

            // Get the API key from environment variables
            var apiKey = Environment.GetEnvironmentVariable("CHOMP_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                return StatusCode(500, new { Message = "API key is not configured. Please set CHOMP_API_KEY in the environment variables." });
            }

            // Construct the API URL
            var url = $"https://chompthis.com/api/v2/food/branded/name.php?api_key={apiKey}&name={Uri.EscapeDataString(name)}&limit={limit}&page={page}";

            try
            {
                // Make the API request
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { Message = "Failed to fetch food data from the external API." });
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var foodData = JsonSerializer.Deserialize<object>(jsonResponse); // Adjust deserialization as needed

                return Ok(foodData); // Return the API response
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error calling external API: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred while fetching food data.", Details = ex.Message });
            }
        }

        [HttpGet("advanced-search")]
        [Authorize]
        public async Task<IActionResult> AdvancedSearchBrandedFood(
    string keyword,
    string category = null,
    string diet = null,
    int limit = 10,
    int page = 1)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { Message = "Keyword cannot be empty." });
            }

            // Get the API key from environment variables
            var apiKey = Environment.GetEnvironmentVariable("CHOMP_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                return StatusCode(500, new { Message = "API key is not configured. Please set CHOMP_API_KEY in the environment variables." });
            }

            // Build query string
            var queryParams = new List<string>
    {
        $"api_key={apiKey}",
        $"keyword={Uri.EscapeDataString(keyword)}",
        $"limit={limit}",
        $"page={page}"
    };

            if (!string.IsNullOrEmpty(category))
                queryParams.Add($"category={Uri.EscapeDataString(category)}");

            if (!string.IsNullOrEmpty(diet))
                queryParams.Add($"diet={Uri.EscapeDataString(diet)}");

            var queryString = string.Join("&", queryParams);
            var url = $"https://chompthis.com/api/v2/food/branded/search.php?{queryString}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { Message = "Failed to fetch food data from the external API." });
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var foodData = JsonSerializer.Deserialize<object>(jsonResponse); // Adjust deserialization as needed

                return Ok(foodData);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error calling external API: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred while fetching food data.", Details = ex.Message });
            }
        }


    }
}
