using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using PantryTracker.Models;
using PantryTracker.Models.DTOs;
using PantryTracker.Data;

namespace PantryTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private PantryTrackerDbContext _dbContext;
    private UserManager<IdentityUser> _userManager;

    public AuthController(PantryTrackerDbContext context, UserManager<IdentityUser> userManager)
    {
        _dbContext = context;
        _userManager = userManager;
    }

    [HttpPost("login")]
    public IActionResult Login([FromHeader(Name = "Authorization")] string authHeader)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic "))
            {
                return BadRequest(new { Message = "Invalid authorization header." });
            }

            string encodedCreds = authHeader.Substring(6).Trim();
            string creds = Encoding
                .GetEncoding("iso-8859-1")
                .GetString(Convert.FromBase64String(encodedCreds));

            // Get email and password
            int separator = creds.IndexOf(':');
            if (separator < 0)
            {
                return BadRequest(new { Message = "Invalid credentials format." });
            }

            string email = creds.Substring(0, separator);
            string password = creds.Substring(separator + 1);

            // Retrieve user from the database
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            var hasher = new PasswordHasher<IdentityUser>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (result != PasswordVerificationResult.Success)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            // Retrieve the user's profile and household information
            var userProfile = _dbContext.UserProfiles.FirstOrDefault(up => up.IdentityUserId == user.Id);
            if (userProfile == null)
            {
                return Unauthorized(new { Message = "User profile not found." });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            // Add roles to claims if any
            var userRoles = _dbContext.UserRoles.Where(ur => ur.UserId == user.Id).ToList();
            foreach (var userRole in userRoles)
            {
                var role = _dbContext.Roles.FirstOrDefault(r => r.Id == userRole.RoleId);
                if (role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)).Wait();

            // Return login success with household information
            return Ok(new
            {
                userId = user.Id,
                firstName = userProfile.FirstName,
                lastName = userProfile.LastName,
                householdId = userProfile.HouseholdId, // This will be null if the user is not in a household
                email = user.Email
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during login: {ex.Message}");
            return StatusCode(500, new { Message = "An error occurred during login.", Exception = ex.Message });
        }
    }

    [HttpGet("logout")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public IActionResult Logout()
    {
        try
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred during logout.", Exception = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized(new { Message = "You are not authenticated" });
        }

        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var profile = _dbContext.UserProfiles.SingleOrDefault(up => up.IdentityUserId == identityUserId);
        if (profile == null)
            return NotFound();

        var userDto = new UserProfileDTO
        {
            Id = profile.Id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            IdentityUserId = identityUserId,
            HouseholdId = profile.HouseholdId,
            HouseholdName = profile.Household?.Name,
            PantryItems = profile.PantryItems?.Select(pi => new PantryItemDTO
            {
                Id = pi.Id,
                Name = pi.Name,
                Quantity = pi.Quantity,
                UpdatedAt = pi.UpdatedAt
            }).ToList()
        };

        return Ok(userDto);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegistrationDTO registration)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Step 1: Create IdentityUser
            var user = new IdentityUser
            {
                Email = registration.Email,
                UserName = registration.Email // Using email as username
            };

            var password = Encoding
                .GetEncoding("iso-8859-1")
                .GetString(Convert.FromBase64String(registration.Password));

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "User creation failed.", Errors = result.Errors });
            }

            // Step 2: Create UserProfile
            var userProfile = new UserProfile
            {
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                IdentityUserId = user.Id
            };

            _dbContext.UserProfiles.Add(userProfile);
            await _dbContext.SaveChangesAsync();

            // Step 3: Determine household assignment
            int? householdId = null;

            if (!string.IsNullOrEmpty(registration.JoinCode))
            {
                // Join existing household
                var household = _dbContext.Households.FirstOrDefault(h => h.JoinCode == registration.JoinCode);
                if (household == null)
                {
                    return BadRequest(new { Message = "Invalid join code." });
                }
                householdId = household.Id;

                // Update the user's HouseholdId
                userProfile.HouseholdId = householdId;
                _dbContext.UserProfiles.Update(userProfile);
                await _dbContext.SaveChangesAsync();
            }
            else if (!string.IsNullOrEmpty(registration.NewHouseholdName))
            {
                // Create new household
                var newHousehold = new Household
                {
                    Name = registration.NewHouseholdName,
                    CreatedAt = DateTime.UtcNow,
                    JoinCode = GenerateUniqueJoinCode(),
                    AdminUserId = userProfile.Id // Set the current user's UserProfile.Id as the AdminUserId
                };
                _dbContext.Households.Add(newHousehold);
                await _dbContext.SaveChangesAsync();
                householdId = newHousehold.Id;

                // Update the user's HouseholdId
                userProfile.HouseholdId = newHousehold.Id;
                _dbContext.UserProfiles.Update(userProfile);
                await _dbContext.SaveChangesAsync();
            }

            // Commit the transaction
            await transaction.CommitAsync();

            // Step 4: Sign in the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Exception during registration: {ex.Message}");
            return StatusCode(500, new { Message = "An error occurred during registration.", Exception = ex.Message });
        }
    }

    private string GenerateUniqueJoinCode()
    {
        string joinCode;
        do
        {
            joinCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        } while (_dbContext.Households.Any(h => h.JoinCode == joinCode));
        return joinCode;
    }
}
