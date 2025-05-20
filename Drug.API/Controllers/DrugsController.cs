using Drug.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.DTOs.Drug;
using PharmaTrack.Shared.Services;
using System.Security.Claims;
using AutoMapper;

namespace Drug.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class DrugsController : ControllerBase
    {
        private readonly DrugDBContext _context;
        private readonly IMapper _mapper;
        public DrugsController(DrugDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetDrugsList(string startWith = "")
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            var isAdmin = User.IsInRole("admin");

            List<DrugListDto>? list;

            if (string.IsNullOrEmpty(startWith))
            {
                list = await _context.DrugProducts
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
                list = await _context.DrugProducts
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
            var drugProduct = await _context.DrugProducts.Where(d => d.DrugCode == drugCode).FirstOrDefaultAsync();

            if (drugProduct == null) return NotFound();

            return Ok(await GetDrugInfoDtoAsync(drugProduct));
        }

        [HttpGet("DIN/{DIN}")]
        public async Task<IActionResult> GetDrugByDIN(string DIN)
        {
            var drugProduct = await _context.DrugProducts.Where(d => d.DrugIdentificationNumber == DIN).FirstOrDefaultAsync();

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
            IQueryable<DrugProduct> query = _context.DrugProducts;
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

        private async Task<ActionResult<DrugInfoDto>> GetDrugInfoDtoAsync(DrugProduct drugProduct)
        {
            /*
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
            */
            // 1. load the product entity
            var product = await _context.DrugProducts
                .FirstOrDefaultAsync(p => p.DrugCode == drugProduct.DrugCode);

            if (product == null) return NotFound();

            // 2. map the root Product
            var dto = new DrugInfoDto
            {
                Product = _mapper.Map<DrugProductDto>(product)
            };

            // 3. fetch & map each child list
            dto.Ingredients = _mapper.Map<List<DrugIngredientDto>>(
                                      await _context.DrugIngredients
                                         .Where(x => x.DrugCode == drugProduct.DrugCode)
                                         .ToListAsync()
                                   );

            dto.Companies = _mapper.Map<List<DrugCompanyDto>>(
                                      await _context.DrugCompanies
                                         .Where(x => x.DrugCode == drugProduct.DrugCode)
                                         .ToListAsync()
                                   );

            dto.Statuses = _mapper.Map<List<DrugStatusDto>>(
                                      await _context.DrugStatuses
                                         .Where(x => x.DrugCode == drugProduct.DrugCode)
                                         .ToListAsync()
                                   );

            dto.Forms = _mapper.Map<List<DrugFormDto>>(
                                      await _context.DrugForms
                                         .Where(x => x.DrugCode == drugProduct.DrugCode)
                                         .ToListAsync()
                                   );

            dto.Packagings = _mapper.Map<List<DrugPackagingDto>>(
                                      await _context.DrugPackagings
                                         .Where(x => x.DrugCode == drugProduct.DrugCode)
                                         .ToListAsync()
                                   );

            dto.PharmaceuticalStds = _mapper.Map<List<DrugPharmaceuticalStdDto>>(
                                      await _context.DrugPharmaceuticalStds
                                         .Where(x => x.DrugCode == drugProduct.DrugCode)
                                         .ToListAsync()
                                   );

            dto.Routes = _mapper.Map<List<DrugRouteDto>>(
                                      await _context.DrugRoutes
                                         .Where(x => x.DrugCode == drugProduct.DrugCode)
                                         .ToListAsync()
                                   );

            dto.Schedules = _mapper.Map<List<DrugScheduleDto>>(
                                      await _context.DrugSchedules
                                         .Where(x => x.DrugCode == drugProduct.DrugCode)
                                         .ToListAsync()
                                   );

            dto.TherapeuticClasses = _mapper.Map<List<DrugTherapeuticClassDto>>(
                                      await _context.DrugTherapeuticClasses
                                         .Where(x => x.DrugCode == drugProduct.DrugCode)
                                         .ToListAsync()
                                   );

            dto.VeterinarySpecies = _mapper.Map<List<DrugVeterinarySpeciesDto>>(
                                      await _context.DrugVeterinarySpecies
                                         .Where(x => x.DrugCode == drugProduct.DrugCode)
                                         .ToListAsync()
                                   );

            // (if you need interactions too)
            // dto.Interactions = _mapper.Map<List<DrugInteractionDto>>(
            //                      await _context.DrugInteractions
            //                         .Where(...).ToListAsync()
            //                    );

            return dto;
        }
    }
}
