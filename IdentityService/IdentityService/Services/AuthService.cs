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
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;
        private readonly IUserServiceClient _userServiceClient;

        public AuthService(UserManager<ApplicationUser> userManager,
            JwtTokenGenerator jwtToken,
            PasswordHasher<ApplicationUser> passwordHasher,
            IUserServiceClient userServiceClient)
        {
            _jwtGenerator = jwtToken;
            _userManager = userManager;
            _passwordHasher = passwordHasher;
            _userServiceClient = userServiceClient;
        }

        public async Task<TokenDto> RegisterAsync(RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.Email,
                FullName = dto.FullName
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(",",result.Errors));
            }

            await _userManager.AddToRoleAsync(user, "Customer");

            IList<string> roles = new List<string>();
            IList<string> permissions = new List<string>();

            try
            {
                var claimsFromUserService = await _userServiceClient.GetUserClaimsByEmailAsync(user.Email);
                if (claimsFromUserService != null)
                {
                    roles = claimsFromUserService.Roles;
                    permissions = claimsFromUserService.Permissions;
                }
            }
            catch
            {
            }

            return new TokenDto
            {
                AccessToken = _jwtGenerator.GenerateToken(user, roles, permissions),
                Expiry = DateTime.UtcNow.AddMinutes(15)
            };
        }

        public async Task<TokenDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) throw new Exception("Invalid Credentials");

            var isValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isValid) throw new Exception("Invalid Credentials");

            // Fetch roles & permissions via Gateway from UserService
            IList<string> roles = new List<string>();
            IList<string> permissions = new List<string>();

            try
            {
                var claimsFromUserService = await _userServiceClient.GetUserClaimsByEmailAsync(user.Email);
                if (claimsFromUserService != null)
                {
                    roles = claimsFromUserService.Roles;
                    permissions = claimsFromUserService.Permissions;
                }
            }
            catch
            {
                // If UserService is down, fallback to Identity roles
                roles = await _userManager.GetRolesAsync(user);
            }

            return new TokenDto
            {
                AccessToken = _jwtGenerator.GenerateToken(user, roles, permissions),
                Expiry = DateTime.UtcNow.AddMinutes(15)
            };
        }
    }
}
