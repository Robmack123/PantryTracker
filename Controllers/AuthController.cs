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
            string encodedCreds = authHeader.Substring(6).Trim();
            string creds = Encoding
                .GetEncoding("iso-8859-1")
                .GetString(Convert.FromBase64String(encodedCreds));

            // Get email and password
            int separator = creds.IndexOf(':');
            string email = creds.Substring(0, separator);
            string password = creds.Substring(separator + 1);

            var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return Unauthorized();

            var userRoles = _dbContext.UserRoles.Where(ur => ur.UserId == user.Id).ToList();
            var hasher = new PasswordHasher<IdentityUser>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (result == PasswordVerificationResult.Success)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName)
                };

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

                return Ok();
            }

            return Unauthorized();
        }
        catch (Exception ex)
        {
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
        var user = new IdentityUser
        {
            Email = registration.Email,
            UserName = registration.Email // Using email as username
        };

        var password = Encoding
            .GetEncoding("iso-8859-1")
            .GetString(Convert.FromBase64String(registration.Password));

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            var userProfile = new UserProfile
            {
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                IdentityUserId = user.Id
            };

            _dbContext.UserProfiles.Add(userProfile);
            await _dbContext.SaveChangesAsync();

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
        return StatusCode(500);
    }
}
