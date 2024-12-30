using Inventory.API.Data;
using PharmaTrack.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers
{
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
            if (request.Name == null || string.IsNullOrWhiteSpace(request.UPC))
            {
                return BadRequest("Product information is required.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check if the product exists
                var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.UPC == request.UPC);

                if (existingProduct != null)
                {
                    // Update the product's fields
                    existingProduct.Name = request.Name ?? existingProduct.Name;
                    existingProduct.NPN = request.NPN ?? existingProduct.NPN;
                    existingProduct.DIN = request.DIN ?? existingProduct.DIN;
                    existingProduct.Brand = request.Brand ?? existingProduct.Brand;
                    existingProduct.UpdatedAt = DateTime.UtcNow;
                    existingProduct.Quantity += request.Type == TransactionType.In ? request.Quantity : -request.Quantity;
                    _context.Products.Update(existingProduct);
                }
                else
                {
                    // Create product if it doesn't exist
                    existingProduct = new Product
                    {
                        UPC = request.UPC,
                        Name = request.Name,
                        NPN = request.NPN,
                        DIN = request.DIN,
                        Brand = request.Brand,
                        Quantity = request.Quantity,
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


