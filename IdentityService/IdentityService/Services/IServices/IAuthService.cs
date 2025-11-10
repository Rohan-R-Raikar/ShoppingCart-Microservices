using IdentityService.Models.DTOs;

namespace IdentityService.Services.IServices
{
    public interface IAuthService
    {
        Task<TokenDto> LoginAsync(LoginDto dto);
        Task<TokenDto> RegisterAsync(RegisterDto dto);
    }
}
