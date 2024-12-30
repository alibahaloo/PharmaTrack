using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Auth.API.Data
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public bool IsAdmin { get; set; } = false;
    }
    public class AuthDBContext : IdentityDbContext
    {
        public AuthDBContext(DbContextOptions<AuthDBContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = default!;
    }
}
