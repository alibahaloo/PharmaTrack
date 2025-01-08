using Inventory.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using PharmaTrack.Shared.Services;


namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public TransactionsController(InventoryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions(int curPage = 1)
        {
            IQueryable<Transaction> query = _context.Transactions.Include(t => t.Product);

            var result = await EFExtensions.GetPaged(query, curPage);

            var response = new PagedResponse<Transaction>
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
        public async Task<IActionResult> GetTransactionById(int id)
        {
            var transaction = await _context.Transactions.Include(t => t.Product).FirstOrDefaultAsync(t => t.Id == id);
            return transaction != null ? Ok(transaction) : NotFound();
        }
    }
}

