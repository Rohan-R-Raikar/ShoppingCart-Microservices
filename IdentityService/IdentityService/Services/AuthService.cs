using IdentityService.JWT;
using IdentityService.Models.DTOs;
using IdentityService.Services.IServices;

namespace IdentityService.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtTokenGenerator _jwtGenerator;
        private readonly IUserServiceClient _userServiceClient;

        public AuthService(
            JwtTokenGenerator jwtToken,
            IUserServiceClient userServiceClient)
        {
            _jwtGenerator = jwtToken;
            _userServiceClient = userServiceClient;
        }

        public async Task<TokenDto> RegisterAsync(RegisterDto dto)
        {
            var userClaims = await _userServiceClient.RegisterUserAsync(dto);
            if (userClaims == null)
            {
                throw new Exception("Registration failed");
            }

            var userdto = new UserDto
            {
                Id = userClaims.UserId,
                Email = userClaims.Email,
                FullName = userClaims.FullName ?? string.Empty,
            };

            return new TokenDto
            {
                AccessToken = _jwtGenerator.GenerateToken(userdto, userClaims.Roles, userClaims.Permissions),
                Expiry = DateTime.UtcNow.AddMinutes(15),
                RefreshToken = null
            };
        }

        // LOGIN
        public async Task<TokenDto> LoginAsync(LoginDto dto)
        {
            var userClaims = await _userServiceClient.VerifyUserCredentialsAsync(dto.Email, dto.Password);
            if (userClaims == null)
            {
                throw new Exception("Invalid Credentials");
            }

            var userdto = new UserDto
            {
                Id = userClaims.UserId,
                Email = userClaims.Email,
                FullName = userClaims.FullName ?? string.Empty,
            };

            return new TokenDto
            {
                AccessToken = _jwtGenerator.GenerateToken(userdto, userClaims.Roles, userClaims.Permissions),
                Expiry = DateTime.UtcNow.AddMinutes(15),
                RefreshToken = null
            };
        }
    }
}
