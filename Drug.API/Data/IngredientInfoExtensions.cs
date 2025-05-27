using Microsoft.EntityFrameworkCore;
using PharmaTrack.Core.DTOs;

namespace Drug.API.Data
{
    public static class IngredientInfoExtensions
    {
        public static async Task<IngredientInfoDto?> ToDtoAsync(this int activeIngredientCode, DrugDBContext context)
        {
            // Get the ingredient name (assuming they are all the same for a given ActiveIngredientCode)
            var ingredientEntry = await context.DrugIngredients
                .Where(di => di.ActiveIngredientCode == activeIngredientCode)
                .Select(di => di.Ingredient)
                .FirstOrDefaultAsync();

            if (ingredientEntry == null) return null;

            // Get all unique drug codes associated with the ingredient
            var drugCodes = await context.DrugIngredients
                .Where(di => di.ActiveIngredientCode == activeIngredientCode)
                .Select(di => di.DrugCode)
                .Distinct()
                .ToListAsync();

            // Build DrugInfoDto list
            var drugInfos = new List<DrugInfoDto>();
            foreach (var drugCode in drugCodes)
            {
                var drugProduct = await context.Drugs.FirstOrDefaultAsync(d => d.DrugCode == drugCode);
                if (drugProduct != null)
                {
                    drugInfos.Add(await drugProduct.ToDtoAsync(context));
                }
            }

            return new IngredientInfoDto
            {
                ActiveIngredientCode = activeIngredientCode,
                Ingredient = ingredientEntry,
                DrugInfos = drugInfos
            };
        }
    }

}
