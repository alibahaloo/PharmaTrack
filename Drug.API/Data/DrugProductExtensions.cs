using Microsoft.EntityFrameworkCore;
using PharmaTrack.Core.DBModels;
using PharmaTrack.Core.DTOs;

namespace Drug.API.Data
{
    public static class DrugProductExtensions
    {
        public static async Task<DrugInfoDto> ToDtoAsync(this DrugProduct product, DrugDBContext context)
        {
            return new DrugInfoDto
            {
                Product = product,
                Ingredients = await context.DrugIngredients.Where(d => d.DrugCode == product.DrugCode).ToListAsync(),
                Companies = await context.DrugCompanies.Where(d => d.DrugCode == product.DrugCode).ToListAsync(),
                Statuses = await context.DrugStatuses.Where(d => d.DrugCode == product.DrugCode).ToListAsync(),
                Forms = await context.DrugForms.Where(d => d.DrugCode == product.DrugCode).ToListAsync(),
                Packagings = await context.DrugPackagings.Where(d => d.DrugCode == product.DrugCode).ToListAsync(),
                PharmaceuticalStds = await context.DrugPharmaceuticalStds.Where(d => d.DrugCode == product.DrugCode).ToListAsync(),
                Routes = await context.DrugRoutes.Where(d => d.DrugCode == product.DrugCode).ToListAsync(),
                Schedules = await context.DrugSchedules.Where(d => d.DrugCode == product.DrugCode).ToListAsync(),
                TherapeuticClasses = await context.DrugTherapeuticClasses.Where(d => d.DrugCode == product.DrugCode).ToListAsync(),
                VeterinarySpecies = await context.DrugVeterinarySpecies.Where(d => d.DrugCode == product.DrugCode).ToListAsync()
            };
        }
    }

}
