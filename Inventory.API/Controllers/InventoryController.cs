using Inventory.API.Data;
using PharmaTrack.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Inventory.API.Controllers
{
    public class StockTransferRequest
    {
        public TransactionType Type { get; set; }
        public Product Product { get; set; } = default!;
        public int Quantity { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public InventoryController(InventoryDbContext context)
        {
            _context = context;
        }

        [HttpPost("stock-transfer")]
        public async Task<IActionResult> StockTransfer([FromBody] StockTransferRequest request)
        {
            if (request.Product == null || string.IsNullOrWhiteSpace(request.Product.UPC))
            {
                return BadRequest("Product information is required.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check if the product exists
                var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.UPC == request.Product.UPC);

                if (existingProduct != null)
                {
                    // Update the product's fields
                    existingProduct.Name = request.Product.Name ?? existingProduct.Name;
                    existingProduct.NPN = request.Product.NPN ?? existingProduct.NPN;
                    existingProduct.DIN = request.Product.DIN ?? existingProduct.DIN;
                    existingProduct.Brand = request.Product.Brand ?? existingProduct.Brand;
                    existingProduct.UpdatedAt = DateTime.UtcNow;

                    _context.Products.Update(existingProduct);
                }
                else
                {
                    // Create product if it doesn't exist
                    existingProduct = new Product
                    {
                        UPC = request.Product.UPC,
                        Name = request.Product.Name,
                        NPN = request.Product.NPN,
                        DIN = request.Product.DIN,
                        Brand = request.Product.Brand,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Products.Add(existingProduct);
                }

                await _context.SaveChangesAsync();

                // Create a new transaction
                var newTransaction = new Transaction
                {
                    ProductId = existingProduct.Id,
                    Type = request.Type,
                    Quantity = request.Quantity,
                    Timestamp = DateTime.UtcNow
                };

                _context.Transactions.Add(newTransaction);
                await _context.SaveChangesAsync();

                // Update or create inventory item
                var inventoryItem = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == existingProduct.Id);

                if (inventoryItem != null)
                {
                    inventoryItem.Quantity += request.Type == TransactionType.In ? request.Quantity : -request.Quantity;
                    inventoryItem.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    inventoryItem = new InventoryItem
                    {
                        ProductId = existingProduct.Id,
                        Quantity = request.Type == TransactionType.In ? request.Quantity : -request.Quantity,
                        LastUpdated = DateTime.UtcNow
                    };

                    _context.InventoryItems.Add(inventoryItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Stock transfer successful.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"An error occurred during stock transfer: {ex.Message}");
            }
        }
    }
}


