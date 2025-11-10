using Microsoft.AspNetCore.Identity;

namespace IdentityService.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // Optional additional attributes
        public string FullName { get; set; }
    }
}
