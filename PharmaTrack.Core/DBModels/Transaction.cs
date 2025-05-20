using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaTrack.Core.DBModels
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; } // Foreign Key to Product

        [Required]
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!; // Navigation Property (Non-nullable)

        [Required]
        public TransactionType Type { get; set; } // Enum: 'In' or 'Out'

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? Reference { get; set; } // Optional external reference

        [Required]
        public string CreatedBy { get; set; } = default!;
    }

    public enum TransactionType
    {
        In = 1,
        Out = 2
    }
}
