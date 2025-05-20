using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Auth.API.Data
{
    public class AuthDBContext : IdentityDbContext
    {
        public AuthDBContext(DbContextOptions<AuthDBContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = default!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;
    }

    public class ApplicationUser : IdentityUser
    {
        [Required]
        public bool IsAdmin { get; set; } = false;
    }

    public class RefreshToken
    {
        public int Id { get; set; } // Primary key
        public string Token { get; set; } = default!;// The refresh token value
        public string UserId { get; set; } = default!;// Foreign key to the user table
        public DateTime ExpiryDate { get; set; } // Expiration time of the token
        public DateTime CreatedDate { get; set; } // When the token was created
    }
}
