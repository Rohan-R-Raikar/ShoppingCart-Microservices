using IdentityService.Models.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.JWT
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _config;
        public JwtTokenGenerator(IConfiguration configuration)
        {
            _config = configuration;
        }

        public string GenerateToken(ApplicationUser user, IList<string> roles, IList<string> permissions)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };
            
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            
            claims.AddRange(permissions.Select(p => new Claim(ClaimTypes.Role, p)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims:claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["JwtSettings:ExpiryMinutes"])),
                signingCredentials:creds
            );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return EncryptString(jwt, _config["JwtSettings:EncryptionKey"]);
        }

        private string EncryptString(String plainText, string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);

            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = key;
            aes.GenerateIV();
            var iv = aes.IV;

            using var encryptor = aes.CreateEncryptor(aes.Key,iv);
            using var ms = new MemoryStream();
            ms.Write(iv,0,iv.Length);
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
