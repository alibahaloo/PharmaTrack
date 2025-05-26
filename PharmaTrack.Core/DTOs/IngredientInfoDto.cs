using PharmaTrack.Core.DBModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaTrack.Core.DTOs
{
    public class IngredientInfoDto
    {
        public List<DrugIngredient> Ingredients { get; set; } = new List<DrugIngredient>();
    }
}
