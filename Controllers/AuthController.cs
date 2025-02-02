using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PantryTracker.Data;
using PantryTracker.Models;
using PantryTracker.Models.DTOs;

namespace PantryTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PantryTrackerDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(PantryTrackerDbContext context, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _dbContext = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        // Helper to generate a JWT token for a given user
        private string GenerateJwtToken(IdentityUser user, UserProfile userProfile)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"]);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim("email", user.Email),
                new Claim("firstName", userProfile.FirstName),
                new Claim("lastName", userProfile.LastName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Optionally, add roles if needed:
            // var roles = await _userManager.GetRolesAsync(user);
            // claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromHeader(Name = "Authorization")] string authHeader)
        {
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic "))
            {
                return BadRequest(new { Message = "Invalid authorization header." });
            }

            var encodedCreds = authHeader.Substring(6).Trim();
            var creds = Encoding.GetEncoding("iso-8859-1").GetString(Convert.FromBase64String(encodedCreds));

            var separatorIndex = creds.IndexOf(':');
            if (separatorIndex < 0)
            {
                return BadRequest(new { Message = "Invalid credentials format." });
            }

            var email = creds.Substring(0, separatorIndex);
            var password = creds.Substring(separatorIndex + 1);

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

            // Generate JWT token
            var token = GenerateJwtToken(user, userProfile);

            return Ok(new
            {
                token = token,
                userId = user.Id,
                firstName = userProfile.FirstName,
                lastName = userProfile.LastName,
                householdId = userProfile.HouseholdId,
                email = user.Email
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegistrationDTO registration)
        {
            registration.JoinCode = registration.JoinCode ?? string.Empty;

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
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

                // Generate JWT token for the new user
                var token = GenerateJwtToken(user, userProfile);

                return Ok(new
                {
                    token = token,
                    userId = user.Id,
                    firstName = userProfile.FirstName,
                    lastName = userProfile.LastName,
                    householdId = userProfile.HouseholdId,
                    email = user.Email
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = "An error occurred during registration.", Exception = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize] // Now secured by JWT
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

        // Helper method for generating a unique join code remains the same
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
