using Inventory.API.Data;
using PharmaTrack.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public ProductsController(InventoryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
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
        public async Task<IActionResult> UpdateProductById(int id, [FromBody] Product updatedProduct)
        {
            if (updatedProduct == null || id != updatedProduct.Id)
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
                // Update fields of the existing product
                existingProduct.UPC = updatedProduct.UPC ?? existingProduct.UPC;
                existingProduct.Name = updatedProduct.Name ?? existingProduct.Name;
                existingProduct.NPN = updatedProduct.NPN ?? existingProduct.NPN;
                existingProduct.DIN = updatedProduct.DIN ?? existingProduct.DIN;
                existingProduct.Brand = updatedProduct.Brand ?? existingProduct.Brand;
                existingProduct.Quantity = updatedProduct.Quantity > 0 ? updatedProduct.Quantity : existingProduct.Quantity;
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
