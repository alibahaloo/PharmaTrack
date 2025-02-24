namespace PharmaTrack.Shared.DTOs
{
    public class DrugInteractionResultDto
    {
        public List<DrugIngredientsDto> Drugs { get; set; } = [];
        public List<InteractionDto> Interactions { get; set; } = [];
    }
    public class InteractionDto
    {
        public string? IngredientA { get; set; }
        public string? IngredientB { get; set; }
        public string? Level { get; set; }
    }
    public class DrugIngredientsDto
    {
        public int DrugCode { get; set; }
        public string? DrugName { get; set; }
        public List<IngredientDto> Ingredients { get; set; } = [];
    }

    public class IngredientDto
    {
        public string Ingredient { get; set; } = default!;
        public bool HasInteraction { get; set; }
    }
}
