using IdentityService.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IdentityService.JWT
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _config;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _config = configuration;
        }

        public string GenerateToken(UserDto user, IList<string> roles, IList<string> permissions)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.FullName)
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            claims.AddRange(permissions.Select(p => new Claim("permission", p)));

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]));
            var encryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:EncryptionKey"]));

            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var encryptingCredentials = new EncryptingCredentials(
                encryptionKey,
                SecurityAlgorithms.Aes256KW,
                SecurityAlgorithms.Aes256CbcHmacSha512
            );

            var handler = new JsonWebTokenHandler();

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"],
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["JwtSettings:ExpiryMinutes"])),
                SigningCredentials = signingCredentials,
                EncryptingCredentials = encryptingCredentials
            };

            return handler.CreateToken(descriptor);
        }
    }
}
