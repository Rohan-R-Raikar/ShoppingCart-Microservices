using IdentityService.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Data
{
    public class ApplicationDbContext 
    {
        public ApplicationDbContext(DbContextOptions options)
        {
        }
    }
}
