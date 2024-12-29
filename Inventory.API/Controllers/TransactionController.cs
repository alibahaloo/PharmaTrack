using Inventory.API.Data;
using Inventory.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class TransactionController : ControllerBase
{
    private readonly InventoryDbContext _context;

    public TransactionController(InventoryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions()
    {
        var transactions = await _context.Transactions.Include(t => t.Product).ToListAsync();
        return Ok(transactions);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction(Transaction transaction)
    {
        var productExists = await _context.Products.AnyAsync(p => p.Id == transaction.ProductId);

        if (!productExists)
        {
            return BadRequest("Invalid ProductId. Product does not exist.");
        }

        transaction.Timestamp = DateTime.UtcNow;

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTransactions), new { id = transaction.Id }, transaction);
    }
}
