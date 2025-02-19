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


        [HttpGet("interactions")]
        public async Task<IActionResult> GetInteractions(string drugCodes)
        {
            // Validate that we received input.
            if (string.IsNullOrWhiteSpace(drugCodes))
            {
                return BadRequest("No drug codes provided.");
            }

            // Split the comma-separated string.
            var codesArray = drugCodes.Split(',', StringSplitOptions.RemoveEmptyEntries);

            // Introduce a maximum limit, e.g., 20 codes.
            const int MAX_CODES = 20;
            if (codesArray.Length > MAX_CODES)
            {
                return BadRequest($"Too many drug codes provided. Maximum allowed is {MAX_CODES}.");
            }

            // Parse each code into an integer.
            var codeList = new List<int>();
            foreach (var code in codesArray)
            {
                if (!int.TryParse(code, out int intCode))
                {
                    return BadRequest($"Invalid drug code: {code}");
                }
                codeList.Add(intCode);
            }

            // Retrieve the ingredients for these drug codes.
            var ingredientNames = await _context.DrugIngredients
                                    .Where(di => codeList.Contains(di.DrugCode))
                                    .Select(di => di.Ingredient)
                                    .ToListAsync();

            // Filter out null/empty values, convert to lower-case, and remove duplicates.
            var lowerIngredients = ingredientNames
                                    .Where(ingredient => !string.IsNullOrEmpty(ingredient))
                                    .Select(ingredient => ingredient!.ToLower())
                                    .Distinct()
                                    .ToList();

            // Query the interactions table based on the ingredients.
            // This example assumes an entity 'DrugInteraction' with properties 'IngredientA' and 'IngredientB'.
            var interactions = await _context.DrugInteractions
                                  .Where(i => (i.DrugA != null && lowerIngredients.Contains(i.DrugA.ToLower())) ||
                                              (i.DrugB != null && lowerIngredients.Contains(i.DrugB.ToLower())))
                                  .ToListAsync();

            return Ok(interactions);
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
