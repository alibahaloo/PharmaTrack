namespace PharmaTrack.Shared.APIModels
{
    public class DrugIngredientRequest
    {
        public int? DrugCode { get; set; }
        public int? ActiveIngredientCode { get; set; }
        public string? Ingredient { get; set; }
    }
}
