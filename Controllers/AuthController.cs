using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PantryTracker.Data;
using PantryTracker.Models;
using PantryTracker.Models.DTOs;

namespace PantryTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private PantryTrackerDbContext _dbContext;
        private UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(PantryTrackerDbContext context, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _dbContext = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromHeader(Name = "Authorization")] string authHeader)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic "))
                {
                    return BadRequest(new { Message = "Invalid authorization header." });
                }

                string encodedCreds = authHeader.Substring(6).Trim();
                string creds = Encoding.GetEncoding("iso-8859-1").GetString(Convert.FromBase64String(encodedCreds));
                int separator = creds.IndexOf(':');
                if (separator < 0)
                {
                    return BadRequest(new { Message = "Invalid credentials format." });
                }
                string email = creds.Substring(0, separator);
                string password = creds.Substring(separator + 1);

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

                var userProfile = _dbContext.UserProfiles.FirstOrDefault(up => up.IdentityUserId == user.Id);
                if (userProfile == null)
                {
                    return Unauthorized(new { Message = "User profile not found." });
                }

                // Generate JWT
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName)
                };
                var jwtSecretKey = _configuration["Jwt:SecretKey"] ?? "YOUR_SECRET_KEY_HERE";
                var issuer = _configuration["Jwt:Issuer"] ?? "YourIssuer";
                var audience = _configuration["Jwt:Audience"] ?? "YourAudience";
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
                var credsSigning = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.Now.AddDays(7),
                    signingCredentials: credsSigning);

                string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                // Return the token and minimal user info
                return Ok(new
                {
                    token = tokenString,
                    user = new
                    {
                        userId = user.Id,
                        firstName = userProfile.FirstName,
                        lastName = userProfile.LastName,
                        householdId = userProfile.HouseholdId,
                        email = user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred during login.", Exception = ex.Message });
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegistrationDTO registration)
        {
            registration.JoinCode = registration.JoinCode ?? string.Empty;

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Create IdentityUser
                var user = new IdentityUser
                {
                    Email = registration.Email,
                    UserName = registration.Email
                };

                var password = Encoding.GetEncoding("iso-8859-1").GetString(Convert.FromBase64String(registration.Password));
                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    return BadRequest(new { Message = "User creation failed.", Errors = result.Errors });
                }

                // Create UserProfile
                var userProfile = new UserProfile
                {
                    FirstName = registration.FirstName,
                    LastName = registration.LastName,
                    IdentityUserId = user.Id
                };

                _dbContext.UserProfiles.Add(userProfile);
                await _dbContext.SaveChangesAsync();

                int? householdId = null;
                if (!string.IsNullOrEmpty(registration.JoinCode))
                {
                    var household = _dbContext.Households.FirstOrDefault(h => h.JoinCode == registration.JoinCode);
                    if (household == null)
                    {
                        return BadRequest(new { Message = "Invalid join code." });
                    }
                    householdId = household.Id;
                    userProfile.HouseholdId = householdId;
                    _dbContext.UserProfiles.Update(userProfile);
                    await _dbContext.SaveChangesAsync();
                }
                else if (!string.IsNullOrEmpty(registration.NewHouseholdName))
                {
                    var newHousehold = new Household
                    {
                        Name = registration.NewHouseholdName,
                        CreatedAt = DateTime.UtcNow,
                        JoinCode = GenerateUniqueJoinCode(),
                        AdminUserId = userProfile.Id
                    };
                    _dbContext.Households.Add(newHousehold);
                    await _dbContext.SaveChangesAsync();
                    householdId = newHousehold.Id;
                    userProfile.HouseholdId = newHousehold.Id;
                    _dbContext.UserProfiles.Update(userProfile);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Sign in the user by generating a JWT
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName)
                };
                var jwtSecretKey = _configuration["Jwt:SecretKey"] ?? "YOUR_SECRET_KEY_HERE";
                var issuer = _configuration["Jwt:Issuer"] ?? "YourIssuer";
                var audience = _configuration["Jwt:Audience"] ?? "YourAudience";
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
                var credsSigning = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.Now.AddDays(7),
                    signingCredentials: credsSigning);

                string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new
                {
                    token = tokenString,
                    user = new
                    {
                        userId = user.Id,
                        firstName = userProfile.FirstName,
                        lastName = userProfile.LastName,
                        householdId = userProfile.HouseholdId,
                        email = user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Exception during registration: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred during registration.", Exception = ex.Message });
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

        [HttpGet("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            return Ok();
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
}
