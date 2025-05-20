using PharmaTrack.Core.DBModels;
using System.Collections.Generic;

namespace PharmaTrack.Core.DTOs
{
    public class DrugInfoDto
    {
        public DrugProduct? Product { get; set; }
        public List<DrugIngredient> Ingredients { get; set; } = new List<DrugIngredient>();
        public List<DrugCompany> Companies { get; set; } = new List<DrugCompany>();
        public List<DrugStatus> Statuses { get; set; } = new List<DrugStatus>();
        public List<DrugForm> Forms { get; set; } = new List<DrugForm>();
        public List<DrugPackaging> Packagings { get; set; } = new List<DrugPackaging>();
        public List<DrugPharmaceuticalStd> PharmaceuticalStds { get; set; } = new List<DrugPharmaceuticalStd>();
        public List<DrugRoute> Routes { get; set; } = new List<DrugRoute>();
        public List<DrugSchedule> Schedules { get; set; } = new List<DrugSchedule>();
        public List<DrugTherapeuticClass> TherapeuticClasses { get; set; } = new List<DrugTherapeuticClass>();
        public List<DrugVeterinarySpecies> VeterinarySpecies { get; set; } = new List<DrugVeterinarySpecies>();
    }

}
