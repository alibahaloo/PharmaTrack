using PharmaTrack.Shared.DBModels;

namespace PharmaTrack.Shared.APIModels
{
    public class DrugInfoDto
    {
        public DrugProduct? Product { get; set; }
        public ICollection<DrugIngredient> Ingredients { get; set; } = [];
        public ICollection<DrugCompany> Companies { get; set; } = [];
        public ICollection<DrugStatus> Statuses { get; set; } = [];
        public ICollection<DrugForm> Forms { get; set; } = [];
        public ICollection<DrugPackaging> Packagings { get; set; } = [];
        public ICollection<DrugPharmaceuticalStd> PharmaceuticalStds { get; set; } = [];
        public ICollection<DrugRoute> Routes { get; set; } = [];
        public ICollection<DrugSchedule> Schedules { get; set; } = [];
        public ICollection<DrugTherapeuticClass> TherapeuticClasses { get; set; } = [];
        public ICollection<DrugVeterinarySpecies> VeterinarySpecies { get; set; } = [];
    }
}
