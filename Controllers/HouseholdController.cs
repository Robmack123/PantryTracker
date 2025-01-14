using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryTracker.Data;
using PantryTracker.Models;
using System.Linq;
using System.Security.Claims;

namespace PantryTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HouseholdController : ControllerBase
{
    private readonly PantryTrackerDbContext _dbContext;

    public HouseholdController(PantryTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET /api/household/members
    [HttpGet("members")]
    [Authorize]
    public IActionResult GetHouseholdMembers()
    {
        try
        {
            // Get the current user's UserProfile
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userProfile = _dbContext.UserProfiles
                .FirstOrDefault(up => up.IdentityUserId == userId);

            if (userProfile == null || userProfile.HouseholdId == null)
            {
                return BadRequest(new { Message = "You are not part of a household." });
            }

            // Fetch the household and its members
            var household = _dbContext.Households
                .Where(h => h.Id == userProfile.HouseholdId)
                .Select(h => new
                {
                    h.JoinCode,
                    h.AdminUserId, // Include the admin user ID
                    Members = h.Users.Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        Email = u.IdentityUser.Email // Assuming navigation to IdentityUser is loaded
                    }).ToList()
                })
                .FirstOrDefault();

            if (household == null)
            {
                return NotFound(new { Message = "Household not found." });
            }

            return Ok(new
            {
                loggedInUserId = userProfile.Id, // Include the logged-in user's ID
                household.JoinCode,
                household.AdminUserId,
                Members = household.Members
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching household members: {ex.Message}");
            return StatusCode(500, new { Message = "An error occurred.", Exception = ex.Message });
        }
    }


    // DELETE /api/household/remove-user/{userId}
    [HttpDelete("remove-user/{userId}")]
    [Authorize]
    public IActionResult RemoveHouseholdUser(int userId)
    {
        try
        {
            // Get the current user's UserProfile
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var adminProfile = _dbContext.UserProfiles
                .FirstOrDefault(up => up.IdentityUserId == adminId);

            if (adminProfile == null || adminProfile.HouseholdId == null)
            {
                return BadRequest(new { Message = "You are not part of a household." });
            }

            // Check if the current user is the admin of the household
            var household = _dbContext.Households
                .FirstOrDefault(h => h.Id == adminProfile.HouseholdId);

            if (household == null || household.AdminUserId != adminProfile.Id)
            {
                return StatusCode(403, new { Message = "You are not authorized to manage this household." });
            }


            // Find the user to remove
            var userToRemove = _dbContext.UserProfiles
                .FirstOrDefault(u => u.Id == userId && u.HouseholdId == household.Id);

            if (userToRemove == null)
            {
                return NotFound(new { Message = "User not found in this household." });
            }

            // Remove the user from the household
            userToRemove.HouseholdId = null;
            _dbContext.UserProfiles.Update(userToRemove);
            _dbContext.SaveChanges();

            return Ok(new { Message = "User removed from the household." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing user: {ex.Message}");
            return StatusCode(500, new { Message = "An error occurred.", Exception = ex.Message });
        }
    }
}
