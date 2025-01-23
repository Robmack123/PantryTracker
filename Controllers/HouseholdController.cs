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
    // POST /api/household/create
    [HttpPost("create")]
    [Authorize]
    public IActionResult CreateHousehold([FromBody] CreateHouseholdDto dto)
    {
        // Log incoming payload
        Console.WriteLine($"[CreateHousehold] Received payload: Name={dto?.Name}, AdminUserId={dto?.AdminUserId}");

        // Validate the payload
        if (dto == null)
        {
            Console.WriteLine("[CreateHousehold] Error: Received null payload.");
            return BadRequest(new { Message = "Invalid payload." });
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            Console.WriteLine("[CreateHousehold] Error: Household Name is missing.");
            return BadRequest(new { Message = "The Name field is required." });
        }

        if (dto.AdminUserId == 0)
        {
            Console.WriteLine("[CreateHousehold] Error: AdminUserId is missing.");
            return BadRequest(new { Message = "The AdminUserId field is required." });
        }

        try
        {
            // Generate a unique join code
            Console.WriteLine("[CreateHousehold] Generating unique join code...");
            string joinCode;
            do
            {
                joinCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            } while (_dbContext.Households.Any(h => h.JoinCode == joinCode));

            // Map DTO to Household
            var newHousehold = new Household
            {
                Name = dto.Name,
                AdminUserId = dto.AdminUserId,
                JoinCode = joinCode,
                CreatedAt = DateTime.UtcNow
            };

            // Add the household to the database
            _dbContext.Households.Add(newHousehold);
            _dbContext.SaveChanges();

            Console.WriteLine($"[CreateHousehold] Household created successfully with ID={newHousehold.Id}");

            // Assign the creator to the household
            var creator = _dbContext.UserProfiles.FirstOrDefault(up => up.Id == dto.AdminUserId);
            if (creator == null)
            {
                Console.WriteLine("[CreateHousehold] Error: Admin user not found.");
                return NotFound(new { Message = "Admin user not found." });
            }

            creator.HouseholdId = newHousehold.Id;
            _dbContext.UserProfiles.Update(creator);
            _dbContext.SaveChanges();

            Console.WriteLine($"[CreateHousehold] Admin user (ID={creator.Id}) assigned to household (ID={newHousehold.Id})");

            // Return success response
            return Ok(new
            {
                HouseholdId = newHousehold.Id,
                HouseholdName = newHousehold.Name,
                JoinCode = newHousehold.JoinCode,
                AdminUserId = newHousehold.AdminUserId
            });
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            Console.WriteLine($"[CreateHousehold] Error creating household: {ex.Message}");
            return StatusCode(500, new { Message = "An error occurred.", Exception = ex.Message });
        }
    }






}
