using Drug.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Core.DBModels;
using PharmaTrack.Core.DTOs;
using PharmaTrack.Shared.Services;
using System.Security.Claims;

namespace Drug.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class DrugsController : ControllerBase
    {
        private readonly DrugDBContext _context;
        public DrugsController(DrugDBContext context)
        {
            _context = context;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetDrugsList(string startWith = "")
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            var isAdmin = User.IsInRole("admin");

            List<DrugListDto>? list;

            if (string.IsNullOrEmpty(startWith))
            {
                list = await _context.Drugs
                    .Select(u => new DrugListDto
                    {
                        Id = u.Id,
                        DrugCode = u.DrugCode,
                        BrandName = u.BrandName
                    })
                    .Distinct()
                    .OrderBy(d => d.BrandName)
                    .Take(10)
                    .ToListAsync();
            } else
            {
                list = await _context.Drugs
                    .Where(u => u.BrandName != null && u.BrandName.StartsWith(startWith))
                    .Select(u => new DrugListDto
                    {
                        Id = u.Id,
                        DrugCode = u.DrugCode,
                        BrandName = u.BrandName
                    })
                    .Distinct()
                    .Take(10)
                    .ToListAsync();
            }

            return Ok(list);
        }

        [HttpGet("{drugCode}")]
        public async Task<IActionResult> GetDrugByCode(int drugCode)
        {
            var drugProduct = await _context.Drugs.Where(d => d.DrugCode == drugCode).FirstOrDefaultAsync();

            if (drugProduct == null) return NotFound();

            var dto = await drugProduct.ToDtoAsync(_context);
            return Ok(dto);
        }

        [HttpGet("DIN/{DIN}")]
        public async Task<IActionResult> GetDrugByDIN(string DIN)
        {
            var drugProduct = await _context.Drugs.Where(d => d.DrugIdentificationNumber == DIN).FirstOrDefaultAsync();

            if (drugProduct == null) return NotFound();

            var dto = await drugProduct.ToDtoAsync(_context);
            return Ok(dto);
        }

        [HttpGet("{drugCode}/ingredients")]
        public async Task<IActionResult> GetIngredientsByDrugCode(int drugCode)
        {
            var drugIngredients = await _context.DrugIngredients
                .Where(di => di.DrugCode == drugCode)
                .Select(di => di.Ingredient)
                .ToListAsync();

            if (drugIngredients == null) return NotFound();

            return Ok(drugIngredients);
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
                )
                    .GroupBy(t => t.BrandName)
                    .Select(g => g.First());
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
    }
}
