using IdentityService.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace IdentityService.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
    }
}
