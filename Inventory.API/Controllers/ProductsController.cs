using Inventory.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Core.DTOs;
using PharmaTrack.Core.DBModels;
using PharmaTrack.Shared.Services;


namespace Inventory.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public ProductsController(InventoryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] string? searchPhrase, int curPage = 1)
        {
            IQueryable<PharmaTrack.Core.DBModels.Product> query = _context.Products;

            if (!string.IsNullOrWhiteSpace(searchPhrase))
            {
                var lower = searchPhrase!.Trim().ToLowerInvariant();
                query = query.Where(p =>
                    p.UPC.ToLower() == lower ||
                    (p.NPN != null && p.NPN.ToLower() == lower) ||
                    (p.DIN != null && p.DIN.ToLower() == lower) ||
                    p.Name.ToLower().Contains(lower) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(lower))
                );
            }

            // always order before paging
            query = query.OrderBy(p => p.Id);

            var result = await EFExtensions.GetPaged(query, curPage);

            var response = new PagedResponse<PharmaTrack.Core.DBModels.Product>
            {
                Data = [.. result.Data], 
                CurrentPage = curPage,
                CurrentItemCount = result.Data.Count,
                TotalPageCount = result.PageCount,
                TotalItemCount = result.RowCount
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            return product != null ? Ok(product) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductById(int id)
        {
            // Find the product by id
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            try
            {
                // Remove the product from the database
                _context.Products.Remove(product);

                // Save changes
                await _context.SaveChangesAsync();

                return Ok($"Product with ID {id} has been deleted successfully.");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during deletion
                return StatusCode(500, $"An error occurred while deleting the product: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProductById(int id, [FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest("Invalid product data.");
            }

            var existingProduct = await _context.Products.FindAsync(id);

            if (existingProduct == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            try
            {
                // Update the fields of the existing product with data from the request
                existingProduct.UPC = product.UPC;
                existingProduct.Name = product.Name;
                existingProduct.NPN = product.NPN;
                existingProduct.DIN = product.DIN;
                existingProduct.Brand = product.Brand;
                //existingProduct.Quantity = updateRequest.Quantity;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                // Save changes to the database
                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();

                return Ok($"Product with ID {id} has been updated successfully.");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the update
                return StatusCode(500, $"An error occurred while updating the product: {ex.Message}");
            }
        }



        [HttpGet("upc/{upc}")]
        public async Task<IActionResult> GetProductByUpc(string upc)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.UPC == upc);
            return product != null ? Ok(product) : NotFound($"Product with UPC '{upc}' not found.");
        }
    }
}
