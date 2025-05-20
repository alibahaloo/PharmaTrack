using System.Collections.Generic;

namespace PharmaTrack.Core.DTOs
{
    public class DrugInteractionResultDto
    {
        public List<InteractionDrugDto> Drugs { get; set; } = new List<InteractionDrugDto>();
        public List<InteractionIngredientDto> Interactions { get; set; } = new List<InteractionIngredientDto>();
    }
    public class IngredientInteractionResultDto
    {
        public List<InteractionIngredientDto> Interactions { get; set; } = new List<InteractionIngredientDto>();
    }
    public class InteractionIngredientDto
    {
        public string? IngredientA { get; set; }
        public string? IngredientB { get; set; }
        public string? Level { get; set; }
    }
    public class InteractionDrugDto
    {
        public int DrugCode { get; set; }
        public string? DrugName { get; set; }
        public List<InteractionDrugIngredientDto> Ingredients { get; set; } = new List<InteractionDrugIngredientDto>();
    }

    public class InteractionDrugIngredientDto
    {
        public string Ingredient { get; set; } = default!;
        public bool HasInteraction { get; set; }
    }
}
