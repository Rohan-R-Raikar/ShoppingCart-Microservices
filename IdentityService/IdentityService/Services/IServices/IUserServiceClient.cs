using IdentityService.Models.DTOs;

namespace IdentityService.Services.IServices
{
    public interface IUserServiceClient
    {
        /// <summary>
        /// Calls the gateway to get user profile + roles + permissions by email.
        /// Returns null if user not found (404).
        /// Throws on other HTTP errors.
        /// </summary>
        Task<UserClaimsDto?> GetUserClaimsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<UserClaimsDto?> VerifyUserCredentialsAsync(string email, string password);
        Task<UserClaimsDto?> RegisterUserAsync(RegisterDto dto);
    }
}