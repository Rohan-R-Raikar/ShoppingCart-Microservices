using IdentityService.Models.DTOs;
using IdentityService.Services.IServices;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace IdentityService.Services
{
    public class UserServiceClient : IUserServiceClient
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public UserServiceClient(HttpClient http)
        {
            _http = http;
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<UserClaimsDto?> GetUserClaimsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var encodedEmail = WebUtility.UrlEncode(email);
            var url = $"user-service/users/{encodedEmail}/claims";

            using var resp = await _http.GetAsync(url, cancellationToken);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            resp.EnsureSuccessStatusCode();

            var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
            var dto = await JsonSerializer.DeserializeAsync<UserClaimsDto>(stream, _jsonOptions, cancellationToken);
            return dto;
        }
        public async Task<UserClaimsDto?> VerifyUserCredentialsAsync(string email, string password)
        {
            try
            {
                var payload = new { email = email, password = password };

                var resp = await _http.PostAsJsonAsync("user-service/verify-credentials", payload);


                if (!resp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[VerifyUserCredentialsAsync] Request failed with status code: {resp.StatusCode}");

                    var content = await resp.Content.ReadAsStringAsync();
                    Console.WriteLine($"[VerifyUserCredentialsAsync] Response content: {content}");

                    return null;
                }

                var userClaims = await resp.Content.ReadFromJsonAsync<UserClaimsDto>(_jsonOptions);

                if (userClaims == null)
                {
                    Console.WriteLine("[VerifyUserCredentialsAsync] Response deserialized to null.");
                }

                return userClaims;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[VerifyUserCredentialsAsync] HTTP request exception: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VerifyUserCredentialsAsync] Unexpected exception: {ex.Message}");
                return null;
            }
        }


        public async Task<UserClaimsDto?> RegisterUserAsync(RegisterDto dto)
        {
            var resp = await _http.PostAsJsonAsync("user-service/users/register", dto);

            if (!resp.IsSuccessStatusCode)
            {
                var content = await resp.Content.ReadAsStringAsync();
                throw new Exception($"UserService registration failed: {resp.StatusCode}, {content}");
            }

            return await resp.Content.ReadFromJsonAsync<UserClaimsDto>(_jsonOptions);
        }

    }
}