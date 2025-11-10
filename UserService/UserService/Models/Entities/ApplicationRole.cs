using Microsoft.AspNetCore.Identity;

namespace UserService.Models.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
