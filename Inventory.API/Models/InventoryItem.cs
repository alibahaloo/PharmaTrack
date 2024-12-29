using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.API.Models
{
    public class InventoryItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; } // Foreign Key to Product

        [Required]
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!; // Navigation Property (Non-nullable)

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
