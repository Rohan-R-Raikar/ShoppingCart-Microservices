using UserService.Models.DTOs;

namespace UserService.Services.IServices
{
    public interface IUserClaimsService
    {
        Task<UserClaimsDto?> GetUserClaimsByEmailAsync(string email);
    }
}
