using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models.DTOs;
using UserService.Models.Entities;
using UserService.Services.IServices;

namespace UserService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserClaimsService _claimsService;
        private readonly ILogger<UsersController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsersController(IUserClaimsService claimsService, ILogger<UsersController> logger,
        UserManager<ApplicationUser> userManager,RoleManager<ApplicationRole> roleManager,ApplicationDbContext context)
        {
            _claimsService = claimsService;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        /// <summary>
        /// GET api/users/{email}/claims
        /// Returns user profile + roles + permissions by email.
        /// 200 OK with empty arrays if user has no roles/permissions.
        /// 404 if user not found.
        /// </summary>
        [HttpGet("{email}/claims")]
        public async Task<ActionResult<UserClaimsDto>> GetUserClaimsByEmail(string email)
        {
            try
            {
                var claims = await _claimsService.GetUserClaimsByEmailAsync(email);
                if (claims == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                // Always return 200 with empty arrays when roles/permissions are missing
                return Ok(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching claims for email {Email}", email);
                return StatusCode(500, new { error = "An error occurred while fetching user claims." });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserClaimsDto>> Register([FromBody] RegisterDto dto)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                    return Conflict(new { error = "Email already exists" });

                var user = new ApplicationUser
                {
                    Email = dto.Email,
                    UserName = dto.Email
                    //FullName = dto.FullName
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                // Assign default role
                await _userManager.AddToRoleAsync(user, "User");

                // Fetch roles and permissions for JWT
                var roles = await _userManager.GetRolesAsync(user);
                var roleIds = await _roleManager.Roles
                    .Where(r => roles.Contains(r.Name))
                    .Select(r => r.Id)
                    .ToListAsync();

                var permissions = await _context.RolePermissions
                    .AsNoTracking()
                    .Include(rp => rp.Permission)
                    .Where(rp => roleIds.Contains(rp.RoleId))
                    .Select(rp => rp.Permission.Name)
                    .Distinct()
                    .ToListAsync();

                var dtoResult = new UserClaimsDto
                {
                    UserId = user.Id,
                    //FullName = user.FullName,
                    Email = user.Email,
                    Roles = roles,
                    Permissions = permissions
                };

                return Ok(dtoResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {Email}", dto.Email);
                return StatusCode(500, new { error = "An error occurred while registering user." });
            }
        }

        [HttpPost("verify-credentials")]
        public async Task<ActionResult<UserClaimsDto>> VerifyCredentials([FromBody] LoginDto dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                    return Unauthorized(new { error = "Invalid credentials" });

                var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
                if (!valid)
                    return Unauthorized(new { error = "Invalid credentials" });

                // Fetch roles and permissions
                var roles = await _userManager.GetRolesAsync(user);
                var roleIds = await _roleManager.Roles
                    .Where(r => roles.Contains(r.Name))
                    .Select(r => r.Id)
                    .ToListAsync();

                var permissions = await _context.RolePermissions
                    .AsNoTracking()
                    .Include(rp => rp.Permission)
                    .Where(rp => roleIds.Contains(rp.RoleId))
                    .Select(rp => rp.Permission.Name)
                    .Distinct()
                    .ToListAsync();

                var dtoResult = new UserClaimsDto
                {
                    UserId = user.Id,
                    //FullName = user.FullName,
                    Email = user.Email,
                    Roles = roles,
                    Permissions = permissions
                };

                return Ok(dtoResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying credentials for {Email}", dto.Email);
                return StatusCode(500, new { error = "An error occurred while verifying credentials." });
            }
        }


    }
}
