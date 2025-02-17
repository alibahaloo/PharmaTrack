using Microsoft.EntityFrameworkCore;
using PharmaTrack.Shared.DBModels;

namespace Drug.API.Data
{
    public class DrugDBContext : DbContext
    {
        public DrugDBContext(DbContextOptions<DrugDBContext> options) : base(options) { }

        // DbSets for all tables
        public DbSet<DrugProduct> Drugs { get; set; } = null!;
        public DbSet<DrugIngredient> DrugIngredients { get; set; } = null!;
        public DbSet<DrugCompany> DrugCompanies { get; set; } = null!;
        public DbSet<DrugStatus> DrugStatuses { get; set; } = null!;
        public DbSet<DrugForm> DrugForms { get; set; } = null!;
        public DbSet<DrugPackaging> DrugPackagings { get; set; } = null!;
        public DbSet<DrugPharmaceuticalStd> DrugPharmaceuticalStds { get; set; } = null!;
        public DbSet<DrugRoute> DrugRoutes { get; set; } = null!;
        public DbSet<DrugSchedule> DrugSchedules { get; set; } = null!;
        public DbSet<DrugTherapeuticClass> DrugTherapeuticClasses { get; set; } = null!;
        public DbSet<DrugVeterinarySpecies> DrugVeterinarySpecies { get; set; } = null!;
        public DbSet<DrugInteraction> DrugInteractions { get; set; }

    }
}
