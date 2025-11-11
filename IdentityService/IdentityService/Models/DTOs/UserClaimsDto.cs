namespace IdentityService.Models.DTOs
{
    public class UserClaimsDto
    {
        public UserDto User { get; set; } = default!;
        public IList<string> Roles { get; set; } = new List<string>();
        public IList<string> Permissions { get; set; } = new List<string>();
    }
}