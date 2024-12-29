using System.ComponentModel.DataAnnotations;

namespace Inventory.API.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string UPC { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? NPN { get; set; }

        [MaxLength(50)]
        public string? DIN { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Brand { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
