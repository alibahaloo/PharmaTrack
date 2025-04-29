using Drug.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using PharmaTrack.Shared.DTOs;
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
            var isAdmin = User.IsInRole("Admin");

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

            return Ok(await GetDrugInfoDtoAsync(drugProduct));
        }

        [HttpGet("DIN/{DIN}")]
        public async Task<IActionResult> GetDrugByDIN(string DIN)
        {
            var drugProduct = await _context.Drugs.Where(d => d.DrugIdentificationNumber == DIN).FirstOrDefaultAsync();

            if (drugProduct == null) return NotFound();

            return Ok(await GetDrugInfoDtoAsync(drugProduct));
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

        private async Task<DrugInfoDto> GetDrugInfoDtoAsync(DrugProduct drugProduct)
        {
            DrugInfoDto drugInfo = new()
            {
                Product = drugProduct,
                Ingredients = await _context.DrugIngredients.Where(d => d.DrugCode == drugProduct.DrugCode).ToListAsync(),
                Companies = await _context.DrugCompanies.Where(d => d.DrugCode == drugProduct.DrugCode).ToListAsync(),
                Statuses = await _context.DrugStatuses.Where(d => d.DrugCode == drugProduct.DrugCode).ToListAsync(),
                Forms = await _context.DrugForms.Where(d => d.DrugCode == drugProduct.DrugCode).ToListAsync(),
                Packagings = await _context.DrugPackagings.Where(d => d.DrugCode == drugProduct.DrugCode).ToListAsync(),
                PharmaceuticalStds = await _context.DrugPharmaceuticalStds.Where(d => d.DrugCode == drugProduct.DrugCode).ToListAsync(),
                Routes = await _context.DrugRoutes.Where(d => d.DrugCode == drugProduct.DrugCode).ToListAsync(),
                Schedules = await _context.DrugSchedules.Where(d => d.DrugCode == drugProduct.DrugCode).ToListAsync(),
                TherapeuticClasses = await _context.DrugTherapeuticClasses.Where(d => d.DrugCode == drugProduct.DrugCode).ToListAsync(),
                VeterinarySpecies = await _context.DrugVeterinarySpecies.Where(d => d.DrugCode == drugProduct.DrugCode).ToListAsync(),
            };

            return drugInfo;
        }
    }
}
