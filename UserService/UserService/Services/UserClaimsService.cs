using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models.DTOs;
using UserService.Models.Entities;
using UserService.Services.IServices;

namespace UserService.Services
{
    public class UserClaimsService : IUserClaimsService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserClaimsService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<UserClaimsDto?> GetUserClaimsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            var roleList = roles ?? new List<string>();

            // Get role ids for the role names (if any)
            var roleIds = new List<string>();
            if (roleList.Any())
            {
                roleIds = await _roleManager.Roles
                    .Where(r => roleList.Contains(r.Name))
                    .Select(r => r.Id)
                    .ToListAsync();
            }

            // Fetch distinct permission names linked to those roleIds
            var permissions = new List<string>();
            if (roleIds.Any())
            {
                permissions = await _context.RolePermissions
                    .AsNoTracking()
                    .Include(rp => rp.Permission)
                    .Where(rp => roleIds.Contains(rp.RoleId))
                    .Select(rp => rp.Permission.Name)
                    .Distinct()
                    .ToListAsync();
            }

            var dto = new UserClaimsDto
            {
                UserId = user.Id,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? email,
                Roles = roleList,
                Permissions = permissions
            };

            // ensure non-null lists
            dto.Roles ??= new List<string>();
            dto.Permissions ??= new List<string>();

            return dto;
        }
    }
}
