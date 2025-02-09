using Drug.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using PharmaTrack.Shared.Services;

namespace Drug.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrugsController : ControllerBase
    {
        private readonly DrugDBContext _context;

        public DrugsController(DrugDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDrugs([FromQuery] DrugInfoRequest request, int curPage = 1)
        {
            IQueryable<DrugProduct> query = _context.Drugs;
            if (request != null)
            {
                query = query.Where(t =>
                    (string.IsNullOrEmpty(request.DIN) || t.DrugIdentificationNumber == request.DIN) &&
                    (string.IsNullOrEmpty(request.BrandName) || ((t.BrandName ?? string.Empty).ToLower().Contains(request.BrandName.ToLower()))) &&
                    (request.DrugCode == null || t.DrugCode == request.DrugCode)
                );
            }
            var result = await EFExtensions.GetPaged(query, curPage);

            var response = new PagedResponse<DrugProduct>
            {
                Data = [.. result.Data],
                CurrentPage = curPage,
                CurrentItemCount = result.Data.Count,
                TotalPageCount = result.PageCount,
                TotalItemCount = result.RowCount
            };

            return Ok(response);
        }
        [HttpGet("{drugCode}")]
        public async Task<IActionResult> GetDrugByCode(int drugCode)
        {
            var drugProduct = await _context.Drugs.Where(d => d.DrugCode == drugCode).FirstOrDefaultAsync();
            return drugProduct != null ? Ok(drugProduct) : NotFound();
        }
    }
}
