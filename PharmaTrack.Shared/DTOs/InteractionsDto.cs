namespace PharmaTrack.Shared.DTOs
{
    public class InteractionResultDto
    {
        public List<InteractionDrugDto> Drugs { get; set; } = [];
        public List<InteractionIngredientDto> Interactions { get; set; } = [];
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
        public List<InteractionDrugIngredientDto> Ingredients { get; set; } = [];
    }

    public class InteractionDrugIngredientDto
    {
        public string Ingredient { get; set; } = default!;
        public bool HasInteraction { get; set; }
    }
}
