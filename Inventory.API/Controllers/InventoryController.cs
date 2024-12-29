using Inventory.API.Data;
using Inventory.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class InventoryController : ControllerBase
{
    private readonly InventoryDbContext _context;

    public InventoryController(InventoryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetInventoryItems()
    {
        var inventoryItems = await _context.InventoryItems.Include(i => i.Product).ToListAsync();
        return Ok(inventoryItems);
    }

    [HttpPost]
    public async Task<IActionResult> AddInventoryItem(InventoryItem inventoryItem)
    {
        var productExists = await _context.Products.AnyAsync(p => p.Id == inventoryItem.ProductId);

        if (!productExists)
        {
            return BadRequest("Invalid ProductId. Product does not exist.");
        }

        inventoryItem.LastUpdated = DateTime.UtcNow;

        _context.InventoryItems.Add(inventoryItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetInventoryItems), new { id = inventoryItem.Id }, inventoryItem);
    }
}
