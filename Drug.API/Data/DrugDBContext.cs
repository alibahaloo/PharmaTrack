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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships and constraints

            // DrugIngredient: Drug (1:Many)
            modelBuilder.Entity<DrugIngredient>()
                .HasOne(di => di.Drug)
                .WithMany(d => d.Ingredients)
                .HasForeignKey(di => di.DrugCode)
                .OnDelete(DeleteBehavior.Cascade);

            // DrugCompany: Drug (1:Many)
            modelBuilder.Entity<DrugCompany>()
                .HasOne(dc => dc.Drug)
                .WithMany(d => d.Companies)
                .HasForeignKey(dc => dc.DrugCode)
                .OnDelete(DeleteBehavior.Cascade);

            // DrugStatus: Drug (1:Many)
            modelBuilder.Entity<DrugStatus>()
                .HasOne(ds => ds.Drug)
                .WithMany(d => d.Statuses)
                .HasForeignKey(ds => ds.DrugCode)
                .OnDelete(DeleteBehavior.Cascade);

            // DrugForm: Drug (1:Many)
            modelBuilder.Entity<DrugForm>()
                .HasOne(df => df.Drug)
                .WithMany(d => d.Forms)
                .HasForeignKey(df => df.DrugCode)
                .OnDelete(DeleteBehavior.Cascade);

            // DrugPackaging: Drug (1:Many)
            modelBuilder.Entity<DrugPackaging>()
                .HasOne(dp => dp.Drug)
                .WithMany(d => d.Packagings)
                .HasForeignKey(dp => dp.DrugCode)
                .OnDelete(DeleteBehavior.Cascade);

            // DrugPharmaceuticalStd: Drug (1:Many)
            modelBuilder.Entity<DrugPharmaceuticalStd>()
                .HasOne(dps => dps.Drug)
                .WithMany(d => d.PharmaceuticalStandards)
                .HasForeignKey(dps => dps.DrugCode)
                .OnDelete(DeleteBehavior.Cascade);

            // DrugRoute: Drug (1:Many)
            modelBuilder.Entity<DrugRoute>()
                .HasOne(dr => dr.Drug)
                .WithMany(d => d.Routes)
                .HasForeignKey(dr => dr.DrugCode)
                .OnDelete(DeleteBehavior.Cascade);

            // DrugSchedule: Drug (1:Many)
            modelBuilder.Entity<DrugSchedule>()
                .HasOne(ds => ds.Drug)
                .WithMany(d => d.Schedules)
                .HasForeignKey(ds => ds.DrugCode)
                .OnDelete(DeleteBehavior.Cascade);

            // DrugTherapeuticClass: Drug (1:Many)
            modelBuilder.Entity<DrugTherapeuticClass>()
                .HasOne(dtc => dtc.Drug)
                .WithMany(d => d.TherapeuticClasses)
                .HasForeignKey(dtc => dtc.DrugCode)
                .OnDelete(DeleteBehavior.Cascade);

            // DrugVeterinarySpecies: Drug (1:Many)
            modelBuilder.Entity<DrugVeterinarySpecies>()
                .HasOne(dvs => dvs.Drug)
                .WithMany(d => d.VeterinarySpecies)
                .HasForeignKey(dvs => dvs.DrugCode)
                .OnDelete(DeleteBehavior.Cascade);

            // Add additional configurations here if needed
        }
    }
}
