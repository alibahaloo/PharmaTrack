using System.Collections.Generic;

namespace PharmaTrack.Core.DTOs
{
    public class IngredientInfoDto
    {
        public string Ingredient { get; set; } = default!;
        public int ActiveIngredientCode { get; set; }
        public List<DrugInfoDto> DrugInfos { get; set; } = new List<DrugInfoDto>();
    }
}
