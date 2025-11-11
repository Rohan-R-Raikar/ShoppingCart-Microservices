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

            // email must be URL encoded
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
    }
}
