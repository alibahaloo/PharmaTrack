using Inventory.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Core.DBModels;
using PharmaTrack.Core.DTOs;
using PharmaTrack.Shared.Services;


namespace Inventory.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public TransactionsController(InventoryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] string? searchPhrase, TransactionType transactionType = TransactionType.In, int curPage = 1)
        {
            IQueryable<Transaction> query = _context.Transactions.Include(t => t.Product);

            if (!string.IsNullOrWhiteSpace(searchPhrase)) {
                var lower = searchPhrase!.Trim().ToLowerInvariant();
                query = query.Where(t =>
                    t.Product.UPC.ToLower() == lower ||
                    (t.Product.NPN != null && t.Product.NPN.ToLower() == lower) ||
                    (t.Product.DIN != null && t.Product.DIN.ToLower() == lower) ||
                    t.Product.Name.ToLower().Contains(lower) ||
                    (t.Product.Brand != null && t.Product.Brand.ToLower().Contains(lower))
                );
            }

            // always order before paging
            query = query.Where(p => p.Type == transactionType).OrderBy(p => p.Id);

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

