using Microsoft.AspNetCore.Identity;
using UserService.Models.Entities;

namespace UserService.Data
{
    public class RoleSeeder
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public RoleSeeder(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task SeedRolesAsync()
        {
            string[] roles = { "Admin", "User" };

            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                }
            }
        }

        public async Task SeedAdminAsync(string email, string password)
        {
            var existingAdmin = await _userManager.FindByEmailAsync(email);
            if (existingAdmin != null) return;

            var adminUser = new ApplicationUser
            {
                Email = email,
                UserName = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, password);
            if (!result.Succeeded)
                throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await _userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
