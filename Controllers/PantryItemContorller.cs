using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryTracker.Data;
using PantryTracker.Models;
using PantryTracker.Models.DTOs;
using System.Linq;
using System.Security.Claims;

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

    }
}