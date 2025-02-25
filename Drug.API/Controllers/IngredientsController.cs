using Drug.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using PharmaTrack.Shared.DTOs;
using PharmaTrack.Shared.Services;

namespace Drug.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientsController : ControllerBase
    {
        private readonly DrugDBContext _context;

        public IngredientsController(DrugDBContext context)
        {
            _context = context;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetDrugsList(string startWith)
        {
            var list = await _context.DrugIngredients
                .Where(u => u.Ingredient != null && u.Ingredient.StartsWith(startWith))
                .Select(u => new IngredientListDto
                {
                    Id = u.Id,
                    Ingredient = u.Ingredient
                }).Distinct()
                .Take(10)
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{ingredientCode}")]
        public async Task<IActionResult> GetIngredientByIngredientCode(int ingredientCode)
        {
            var drugIngredients = await _context.DrugIngredients.Where(di => di.ActiveIngredientCode == ingredientCode).ToListAsync();

            if (drugIngredients == null) return NotFound();

            return Ok(drugIngredients);
        }

        [HttpGet]
        public async Task<IActionResult> GetIngredients([FromQuery] DrugIngredientRequest request, int curPage = 1)
        {
            IQueryable<DrugIngredient> query = _context.DrugIngredients;

            if (request != null)
            {
                query = query.Where(
                    t => (string.IsNullOrEmpty(request.Ingredient) || (t.Ingredient != null && t.Ingredient.ToLower().Contains(request.Ingredient.ToLower()))) &&
                    (request.DrugCode == null || t.DrugCode == request.DrugCode) &&
                    (request.ActiveIngredientCode == null || t.ActiveIngredientCode == request.ActiveIngredientCode)
                );
            }
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
