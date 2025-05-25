namespace PharmaTrack.Core.DTOs
{
    public class DrugIngredientQuery
    {
        public int? DrugCode { get; set; }
        public int? ActiveIngredientCode { get; set; }
        public string? Ingredient { get; set; }
    }
}
