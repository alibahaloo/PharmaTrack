using Inventory.API.Data;
using PharmaTrack.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Inventory.API.Controllers
{
    public class AddProductRequest
    {
        public string UPC { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? NPN { get; set; }
        public string? DIN { get; set; }
        public string? Brand { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public ProductController(InventoryDbContext context)
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

        [HttpGet("upc/{upc}")]
        public async Task<IActionResult> GetProductByUpc(string upc)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.UPC == upc);
            return product != null ? Ok(product) : NotFound($"Product with UPC '{upc}' not found.");
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] AddProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UPC) || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Both UPC and Name are required.");
            }

            var product = new Product
            {
                UPC = request.UPC,
                Name = request.Name,
                NPN = request.NPN,
                DIN = request.DIN,
                Brand = request.Brand,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }
    }
}
