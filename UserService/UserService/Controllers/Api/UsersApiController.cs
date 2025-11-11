using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.DTOs;
using UserService.Services.IServices;

namespace UserService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserClaimsService _claimsService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserClaimsService claimsService, ILogger<UsersController> logger)
        {
            _claimsService = claimsService;
            _logger = logger;
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
    }
}
