using Drug.API.Data;
using PharmaTrack.Core.DBModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Core.DTOs;

namespace Drug.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
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

            // Retrieve all ingredient names for the provided drug codes.
            var allIngredientsData = await _context.DrugIngredients
                .Where(di => codeList.Contains(di.DrugCode))
                .Select(di => new { di.DrugCode, di.Ingredient })
                .ToListAsync();

            // Prepare the list of ingredients (lower-cased, distinct) for interactions.
            var lowerIngredients = allIngredientsData
                .Where(x => !string.IsNullOrWhiteSpace(x.Ingredient))
                .Select(x => x.Ingredient!.ToLower())
                .Distinct()
                .ToList();

            // Retrieve drug names for these drug codes.
            var drugsInfo = await _context.Drugs
                .Where(d => codeList.Contains(d.DrugCode))
                .Select(d => new { d.DrugCode, d.BrandName })
                .ToListAsync();

            // Prepare a list to hold the interactions.
            List<DrugInteraction> interactions = [];

            if (lowerIngredients.Count == 0)
            {
                // No valid ingredients found.
                interactions = [];
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
                interactions = [.. interactionSet];
            }

            // Build the first part of the result: group drugs with their ingredients.
            var drugsDto = allIngredientsData
                .GroupBy(x => x.DrugCode)
                .Select(g => new InteractionDrugDto
                {
                    DrugCode = g.Key,
                    DrugName = drugsInfo.FirstOrDefault(di => di.DrugCode == g.Key)?.BrandName,
                    Ingredients = g.Where(x => !string.IsNullOrWhiteSpace(x.Ingredient))
                                   .Select(x => x.Ingredient!.ToLower())
                                   .Distinct()
                                   .Select(ing => new InteractionDrugIngredientDto { Ingredient = ing })
                                   .ToList()
                })
                .ToList();

            // Create a set of ingredients that are involved in an interaction.
            var interactionIngredients = new HashSet<string>();
            foreach (var inter in interactions)
            {
                if (!string.IsNullOrWhiteSpace(inter.DrugA))
                {
                    interactionIngredients.Add(inter.DrugA.ToLower());
                }
                if (!string.IsNullOrWhiteSpace(inter.DrugB))
                {
                    interactionIngredients.Add(inter.DrugB.ToLower());
                }
            }

            // For each drug's ingredient, mark if it has an interaction.
            foreach (var drug in drugsDto)
            {
                foreach (var ing in drug.Ingredients)
                {
                    ing.HasInteraction = interactionIngredients.Contains(ing.Ingredient);
                }
            }

            var interactionsDto = interactions.Select(interaction => new InteractionIngredientDto
            {
                IngredientA = interaction.DrugA,
                IngredientB = interaction.DrugB,
                Level = interaction.Level,
            }).ToList();

            // Compose the final result with both parts.
            var result = new DrugInteractionResultDto
            {
                Drugs = drugsDto,
                Interactions = interactionsDto
            };

            return Ok(result);
        }

        [HttpGet("ingredients/{ingredients}")]
        public async Task<IActionResult> GetInteractionsByIngredientCode(string ingredients)
        {
            // Validate input.
            if (string.IsNullOrWhiteSpace(ingredients))
            {
                return BadRequest("No ingredients provided.");
            }

            // Split by comma without removing empty entries.
            var tokens = ingredients.Split(new[] { ',' }, StringSplitOptions.None);

            // Trim each token.
            var trimmedTokens = tokens.Select(t => t.Trim()).ToList();

            // Check for any empty tokens.
            if (trimmedTokens.Any(t => string.IsNullOrEmpty(t)))
                return BadRequest("Empty tokens are not allowed in the input.");

            List<string> loweredIngredientNames = trimmedTokens.Select(t => t.ToLowerInvariant()).ToList();

            // Maximum limit
            const int MAX_CODES = 11;
            if (loweredIngredientNames.Count > MAX_CODES)
            {
                return BadRequest($"Too many ingredient codes provided. Maximum allowed is {MAX_CODES}.");
            }

            // Prepare a list to hold the interactions.
            List<DrugInteraction> interactions = [];

            if (loweredIngredientNames.Count == 0)
            {
                // No valid ingredients found.
                interactions = [];
            }
            else if (loweredIngredientNames.Count == 1)
            {
                // If there's only one ingredient, return interactions where either side matches.
                string ingredient = loweredIngredientNames.First();
                interactions = await _context.DrugInteractions
                    .Where(i => (i.DrugA != null && i.DrugA.ToLower() == ingredient) ||
                                (i.DrugB != null && i.DrugB.ToLower() == ingredient))
                    .ToListAsync();
            }
            else if (loweredIngredientNames.Count == 2)
            {
                // If there are two ingredients, return interactions where both are present.
                string ingredient1 = loweredIngredientNames[0];
                string ingredient2 = loweredIngredientNames[1];
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
                for (int i = 0; i < loweredIngredientNames.Count - 1; i++)
                {
                    for (int j = i + 1; j < loweredIngredientNames.Count; j++)
                    {
                        string ingredient1 = loweredIngredientNames[i];
                        string ingredient2 = loweredIngredientNames[j];
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
                interactions = [.. interactionSet];
            }

            var interactionsDto = interactions.Select(interaction => new InteractionIngredientDto
            {
                IngredientA = interaction.DrugA,
                IngredientB = interaction.DrugB,
                Level = interaction.Level,
            }).ToList();

            var result = new IngredientInteractionResultDto {
                Interactions = interactionsDto 
            };

            return Ok(result);
        }
    }
}
