using Microsoft.AspNetCore.Identity;

namespace UserService.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
