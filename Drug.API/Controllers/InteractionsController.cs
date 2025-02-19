using Drug.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Shared.DBModels;

namespace Drug.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InteractionsController : ControllerBase
    {
        private readonly DrugDBContext _context;

        public InteractionsController(DrugDBContext context)
        {
            _context = context;
        }

        [HttpGet("drugs/{drugCodes}")]
        public async Task<IActionResult> GetInteractionsByDrugCode(string drugCodes)
        {
            // Validate input.
            if (string.IsNullOrWhiteSpace(drugCodes))
            {
                return BadRequest("No drug codes provided.");
            }

            // Split the comma-separated string.
            var codesArray = drugCodes.Split(',', StringSplitOptions.RemoveEmptyEntries);

            // Maximum limit, e.g., 20 codes.
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
                                    .Where(ingredient => !string.IsNullOrWhiteSpace(ingredient))
                                    .Select(ingredient => ingredient!.ToLower())
                                    .Distinct()
                                    .ToList();

            // Prepare a list to hold the interactions.
            List<DrugInteraction> interactions = [];

            if (lowerIngredients.Count == 0)
            {
                // No valid ingredients found.
                return Ok(interactions);
            }
            else if (lowerIngredients.Count == 1)
            {
                // If there's only one ingredient, return interactions where either side matches.
                string ingredient = lowerIngredients.First();
                interactions = await _context.DrugInteractions
                    .Where(i => (i.DrugA != null && i.DrugA.ToLower() == ingredient) ||
                                (i.DrugB != null && i.DrugB.ToLower() == ingredient))
                    .ToListAsync();
            }
            else if (lowerIngredients.Count == 2)
            {
                // If there are two ingredients, return interactions where both are present.
                string ingredient1 = lowerIngredients[0];
                string ingredient2 = lowerIngredients[1];
                interactions = await _context.DrugInteractions
                    .Where(i => i.DrugA != null && i.DrugB != null &&
                                ((i.DrugA.ToLower() == ingredient1 && i.DrugB.ToLower() == ingredient2) ||
                                 (i.DrugA.ToLower() == ingredient2 && i.DrugB.ToLower() == ingredient1)))
                    .ToListAsync();
            }
            else
            {
                // For more than two ingredients, loop through every unique pair.
                var interactionSet = new HashSet<DrugInteraction>(); // Use a set to avoid duplicates.
                for (int i = 0; i < lowerIngredients.Count - 1; i++)
                {
                    for (int j = i + 1; j < lowerIngredients.Count; j++)
                    {
                        string ingredient1 = lowerIngredients[i];
                        string ingredient2 = lowerIngredients[j];
                        var pairInteractions = await _context.DrugInteractions
                            .Where(interaction => interaction.DrugA != null && interaction.DrugB != null &&
                                  ((interaction.DrugA.ToLower() == ingredient1 && interaction.DrugB.ToLower() == ingredient2) ||
                                   (interaction.DrugA.ToLower() == ingredient2 && interaction.DrugB.ToLower() == ingredient1)))
                            .ToListAsync();

                        foreach (var inter in pairInteractions)
                        {
                            interactionSet.Add(inter);
                        }
                    }
                }
                interactions = interactionSet.ToList();
            }

            return Ok(interactions);
        }

    }
}
