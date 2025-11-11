using Microsoft.EntityFrameworkCore;
using UserService.Models.Entities;

namespace UserService.Data
{
    public class PermissionSeeder
    {
        private readonly ApplicationDbContext _context;

        public PermissionSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            var permissions = new[] {
                "CanAddProduct", "CanEditProduct", "CanDeleteProduct",
                "CanAddCategory", "CanEditCategory", "CanDeleteCategory",
                "CanAddCart","CanEditCart","CanDeleteCart"
            };

            foreach (var perm in permissions)
            {
                if (!await _context.Permissions.AnyAsync(p => p.Name == perm))
                {
                    _context.Permissions.Add(new Permission { Name = perm });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
