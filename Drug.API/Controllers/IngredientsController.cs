using Drug.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Core.DBModels;
using PharmaTrack.Core.DTOs;
using PharmaTrack.Shared.Services;

namespace Drug.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IngredientsController : ControllerBase
    {
        private readonly DrugDBContext _context;

        public IngredientsController(DrugDBContext context)
        {
            _context = context;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetIngredientsList(string startWith = "")
        {
            List<string> list;

            if (string.IsNullOrEmpty(startWith))
            {
                list = await _context.DrugInteractions
                    .Where(di => di.DrugA != null)
                    .Select(di => di.DrugA!)
                    .Distinct() // Ensure uniqueness first
                    .OrderBy(DrugA => DrugA) // Then apply ordering
                    .Take(10)
                    .ToListAsync();

            }
            else
            {
                list = await _context.DrugInteractions
                    .Where(di => di.DrugA != null && di.DrugA.StartsWith(startWith))
                    .Select(di => di.DrugA!)
                    .Distinct()
                    .OrderBy(DrugA => DrugA)
                    .Take(10)
                    .ToListAsync();
            }
                
            return Ok(list);
        }

        [HttpGet("{ingredientCode}")]
        public async Task<ActionResult<IngredientInfoDto>> GetIngredientByIngredientCode(int ingredientCode)
        {
            var ingredientInfo = await ingredientCode.ToDtoAsync(_context);

            if (ingredientInfo == null)
                return NotFound();

            return Ok(ingredientInfo);
        }

        [HttpGet]
        public async Task<IActionResult> GetIngredients([FromQuery] string? searchPhrase, int curPage = 1)
        {
            IQueryable<DrugIngredient> query = _context.DrugIngredients;

            if (!string.IsNullOrWhiteSpace(searchPhrase))
            {
                var lower = searchPhrase!.Trim().ToLowerInvariant();
                var isNumeric = int.TryParse(lower, out var code);

                query = query
                    .Where(t =>
                        // substring match on Ingredient
                        ((t.Ingredient ?? "")
                           .ToLower()
                           .Contains(lower))

                     // exact match on DrugCode if numeric
                     || (isNumeric && t.DrugCode == code)

                     // exact match on ActiveIngredientCode if numeric
                     || (isNumeric && t.ActiveIngredientCode == code)
                    );
            }

            // 3) enforce a stable OrderBy before paging
            query = query.OrderBy(t => t.Id).GroupBy(t => t.Ingredient).Select(g => g.First());

            var result = await EFExtensions.GetPaged(query, curPage);
            var response = new PagedResponse<DrugIngredient>
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
