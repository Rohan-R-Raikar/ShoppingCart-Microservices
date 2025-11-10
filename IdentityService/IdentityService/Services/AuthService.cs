using IdentityService.JWT;
using IdentityService.Models.DTOs;
using IdentityService.Models.Entities;
using IdentityService.Services.IServices;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services
{
    public class AuthService :IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtTokenGenerator _jwtGenerator;

        public AuthService(UserManager<ApplicationUser> userManager,
            JwtTokenGenerator jwtToken)
        {
            _jwtGenerator = jwtToken;
            _userManager = userManager;
        }

        public async Task<TokenDto> RegisterAsync(RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.Email,
                FullName = dto.FullName
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(",",result.Errors));
            }
            await _userManager.AddToRoleAsync(user, "Customer");
            var roles = await _userManager.GetRolesAsync(user);
            var permission = new List<string>();

            return new TokenDto
            {
                AccessToken = _jwtGenerator.GenerateToken(user, roles, permission),
                Expiry = DateTime.UtcNow.AddMinutes(15)
            };
        }

        public async Task<TokenDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) throw new Exception("Invalid Credentials");

            var isValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isValid) throw new Exception("Invalid Credentials");

            var roles = await _userManager.GetRolesAsync(user);
            var permission = new List<string>();

            return new TokenDto
            {
                AccessToken = _jwtGenerator.GenerateToken(user,roles, permission),
                Expiry= DateTime.UtcNow.AddMinutes(15)
            };
        }
    }
}
