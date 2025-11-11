namespace IdentityService.Models.DTOs
{
    public class UserClaimsDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public IList<string> Permissions { get; set; } = new List<string>();
    }
}
