﻿using System.ComponentModel.DataAnnotations;
using PharmaTrack.Core.DBModels;
namespace PharmaTrack.Core.DTOs
{
    public class StockTransferRequest
    {
        [Required(ErrorMessage = "Stock Transfer Type is required.")]
        public TransactionType Type { get; set; }

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
        [Required, Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
