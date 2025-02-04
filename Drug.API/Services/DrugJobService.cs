using Drug.API.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PharmaTrack.Shared.DBModels;
using System.Linq;

namespace Drug.API.Services
{
    public class DrugJobService
    {
        private readonly DrugDBContext _context;

        public DrugJobService(DrugDBContext context)
        {
            _context = context;
        }

        public async Task ProcessDrugDataAsync()
        {
            Console.WriteLine($"Processing Drugs");
            await Task.Delay(500);
            Console.WriteLine($"Processing Done");
        }

        public async Task ImportDataAsync()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string filePath = "/app/DataFiles/DPD.xlsx";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Excel file not found at {filePath}");
            }

            using var package = new ExcelPackage(new FileInfo(filePath));

            await ImportDrugs(package);
            await ImportDrugIngredients(package);
            await ImportDrugCompanies(package);
            await ImportDrugStatus(package);
            await ImportDrugForms(package);
            await ImportDrugPackaging(package);
            await ImportDrugPharmaceuticalStds(package);
            await ImportDrugRoutes(package);
            await ImportDrugSchedules(package);
            await ImportDrugTherapeuticClasses(package);
            await ImportDrugVeterinarySpecies(package);

            await _context.SaveChangesAsync();
        }
        private async Task ImportDrugVeterinarySpecies(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["vet"];
            if (sheet == null) return;

            var veterinarySpecies = new List<DrugVeterinarySpecies>();

            var existingSpecies = await _context.DrugVeterinarySpecies
                .AsNoTracking()
                .Select(vs => new { vs.DrugCode, vs.VetSpecies, vs.VetSubSpecies })
                .ToHashSetAsync();

            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                int.TryParse(sheet.Cells[row, 1].Text.Trim(), out int drugCode);
                var vetSpecies = sheet.Cells[row, 2].Text.Trim();
                var vetSubSpecies = sheet.Cells[row, 3].Text.Trim();

                if (drugCode == 0 && string.IsNullOrWhiteSpace(vetSpecies))
                    continue;

                if (existingSpecies.Contains(new { DrugCode = drugCode, VetSpecies = vetSpecies, VetSubSpecies = vetSubSpecies }))
                    continue;

                veterinarySpecies.Add(new DrugVeterinarySpecies
                {
                    DrugCode = drugCode,
                    VetSpecies = vetSpecies,
                    VetSubSpecies = vetSubSpecies
                });

                existingSpecies.Add(new { DrugCode = drugCode, VetSpecies = vetSpecies, VetSubSpecies = vetSubSpecies });
            }

            if (veterinarySpecies.Count > 0)
            {
                await _context.DrugVeterinarySpecies.AddRangeAsync(veterinarySpecies);
            }
        }


        private async Task ImportDrugTherapeuticClasses(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["ther"];
            if (sheet == null) return;

            var therapeuticClasses = new List<DrugTherapeuticClass>();

            // Fetch existing therapeutic classes to avoid duplicates
            var existingClasses = await _context.DrugTherapeuticClasses
                .AsNoTracking()
                .Select(tc => new { tc.DrugCode, tc.TcAtcNumber, tc.TcAtc, tc.TcAhfsNumber, tc.TcAhfs })
                .ToHashSetAsync();

            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                int.TryParse(sheet.Cells[row, 1].Text.Trim(), out int drugCode);
                var tcAtcNumber = sheet.Cells[row, 2].Text.Trim();
                var tcAtc = sheet.Cells[row, 3].Text.Trim();
                var tcAhfsNumber = sheet.Cells[row, 4].Text.Trim();
                var tcAhfs = sheet.Cells[row, 5].Text.Trim();

                if (drugCode == 0 && string.IsNullOrWhiteSpace(tcAtc))
                    continue;

                // Check if this combination already exists
                if (existingClasses.Contains(new { DrugCode = drugCode, TcAtcNumber = tcAtcNumber, TcAtc = tcAtc, TcAhfsNumber = tcAhfsNumber, TcAhfs = tcAhfs }))
                    continue;

                therapeuticClasses.Add(new DrugTherapeuticClass
                {
                    DrugCode = drugCode,
                    TcAtcNumber = tcAtcNumber,
                    TcAtc = tcAtc,
                    TcAhfsNumber = tcAhfsNumber,
                    TcAhfs = tcAhfs
                });

                // Add to HashSet to prevent tracking conflicts
                existingClasses.Add(new { DrugCode = drugCode, TcAtcNumber = tcAtcNumber, TcAtc = tcAtc, TcAhfsNumber = tcAhfsNumber, TcAhfs = tcAhfs });
            }

            if (therapeuticClasses.Count > 0)
            {
                await _context.DrugTherapeuticClasses.AddRangeAsync(therapeuticClasses);
            }
        }


        private async Task ImportDrugSchedules(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["schedule"];
            if (sheet == null) return;

            var schedules = new List<DrugSchedule>();

            // Fetch existing schedule entries to avoid duplicates
            var existingSchedules = await _context.DrugSchedules
                .AsNoTracking()
                .Select(ds => new { ds.DrugCode, ds.Schedule })
                .ToHashSetAsync();

            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                int.TryParse(sheet.Cells[row, 1].Text.Trim(), out int drugCode);
                var scheduleText = sheet.Cells[row, 2].Text.Trim();

                if (drugCode == 0 && string.IsNullOrWhiteSpace(scheduleText))
                    continue;

                // Check if this combination already exists
                if (existingSchedules.Contains(new { DrugCode = drugCode, Schedule = scheduleText }))
                    continue;

                schedules.Add(new DrugSchedule
                {
                    DrugCode = drugCode,
                    Schedule = scheduleText
                });

                // Add to HashSet to prevent tracking conflicts
                existingSchedules.Add(new { DrugCode = drugCode, Schedule = scheduleText });
            }

            if (schedules.Count > 0)
            {
                await _context.DrugSchedules.AddRangeAsync(schedules);
            }
        }


        private async Task ImportDrugRoutes(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["route"];
            if (sheet == null) return;

            var routes = new List<DrugRoute>();

            // Fetch existing route entries to avoid duplicates
            var existingRoutes = await _context.DrugRoutes
                .AsNoTracking()
                .Select(dr => new { dr.DrugCode, dr.RouteOfAdministration })
                .ToHashSetAsync();

            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                int.TryParse(sheet.Cells[row, 1].Text.Trim(), out int drugCode);
                var routeAdmin = sheet.Cells[row, 3].Text.Trim();

                if (drugCode == 0 && string.IsNullOrWhiteSpace(routeAdmin))
                    continue;

                // Check if this combination already exists
                if (existingRoutes.Contains(new { DrugCode = drugCode, RouteOfAdministration = routeAdmin }))
                    continue;

                routes.Add(new DrugRoute
                {
                    DrugCode = drugCode,
                    RouteOfAdministration = routeAdmin
                });

                // Add to HashSet to prevent tracking conflicts
                existingRoutes.Add(new { DrugCode = drugCode, RouteOfAdministration = routeAdmin });
            }

            if (routes.Count > 0)
            {
                await _context.DrugRoutes.AddRangeAsync(routes);
            }
        }


        private async Task ImportDrugPackaging(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["package"];
            if (sheet == null) return;

            var packagings = new List<DrugPackaging>();

            // Fetch existing packaging entries to avoid duplicates
            var existingPackagings = await _context.DrugPackagings
                .AsNoTracking()
                .Select(dp => new { dp.DrugCode, dp.PackageType, dp.PackageSizeUnit, dp.PackageSize, dp.ProductInformation })
                .ToHashSetAsync();

            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                int.TryParse(sheet.Cells[row, 1].Text.Trim(), out int drugCode);
                var packageType = sheet.Cells[row, 4].Text.Trim();
                var packageSizeUnit = sheet.Cells[row, 3].Text.Trim();
                var packageSize = sheet.Cells[row, 5].Text.Trim();
                var productInfo = sheet.Cells[row, 6].Text.Trim();

                if (drugCode == 0 && string.IsNullOrWhiteSpace(packageType))
                    continue;

                // Check if this combination already exists
                if (existingPackagings.Contains(new { DrugCode = drugCode, PackageType = packageType, PackageSizeUnit = packageSizeUnit, PackageSize = packageSize, ProductInformation = productInfo }))
                    continue;

                packagings.Add(new DrugPackaging
                {
                    DrugCode = drugCode,
                    PackageSizeUnit = packageSizeUnit,
                    PackageType = packageType,
                    PackageSize = packageSize,
                    ProductInformation = productInfo
                });

                // Add to HashSet to prevent tracking conflicts
                existingPackagings.Add(new { DrugCode = drugCode, PackageType = packageType, PackageSizeUnit = packageSizeUnit, PackageSize = packageSize, ProductInformation = productInfo });
            }

            if (packagings.Count > 0)
            {
                await _context.DrugPackagings.AddRangeAsync(packagings);
            }
        }


        private async Task ImportDrugs(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["drug"];
            if (sheet == null) return;

            var drugs = new List<DrugProduct>();

            // Fetch existing drugs to avoid duplicates
            var existingDrugs = await _context.Drugs
                .AsNoTracking()
                .Select(d => d.DrugCode)
                .ToHashSetAsync();

            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                int.TryParse(sheet.Cells[row, 1].Text.Trim(), out int drugCode);
                var brandName = sheet.Cells[row, 5].Text.Trim();

                if (drugCode == 0 && string.IsNullOrWhiteSpace(brandName))
                    continue;

                // Skip if the DrugCode already exists
                if (existingDrugs.Contains(drugCode))
                    continue;

                drugs.Add(new DrugProduct
                {
                    DrugCode = drugCode,
                    ProductCategorization = sheet.Cells[row, 2].Text.Trim(),
                    Class = sheet.Cells[row, 3].Text.Trim(),
                    DrugIdentificationNumber = sheet.Cells[row, 4].Text.Trim(),
                    BrandName = brandName,
                    Descriptor = sheet.Cells[row, 6].Text.Trim(),
                    PediatricFlag = sheet.Cells[row, 7].Text.Trim(),
                    AccessionNumber = sheet.Cells[row, 8].Text.Trim(),
                    NumberOfAis = sheet.Cells[row, 9].Text.Trim(),
                    LastUpdateDate = DateTime.TryParse(sheet.Cells[row, 10].Text.Trim(), out var date) ? date : null
                });

                // Add to HashSet to prevent tracking conflicts
                existingDrugs.Add(drugCode);
            }

            if (drugs.Count > 0)
            {
                await _context.Drugs.AddRangeAsync(drugs);
            }
        }


        private async Task ImportDrugIngredients(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["ingred"];
            if (sheet == null) return;

            var ingredients = new List<DrugIngredient>();

            // Fetch existing ingredient entries to avoid duplicates
            var existingIngredients = await _context.DrugIngredients
                .AsNoTracking()
                .Select(di => new { di.DrugCode, di.ActiveIngredientCode, di.Ingredient })
                .ToHashSetAsync();

            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                int.TryParse(sheet.Cells[row, 1].Text.Trim(), out int drugCode);
                int.TryParse(sheet.Cells[row, 2].Text.Trim(), out int activeIngredientCode);
                var ingredient = sheet.Cells[row, 3].Text.Trim();
                var strength = sheet.Cells[row, 5].Text.Trim();
                var strengthUnit = sheet.Cells[row, 6].Text.Trim();
                var strengthType = sheet.Cells[row, 7].Text.Trim();

                if (drugCode == 0 && activeIngredientCode == 0 && string.IsNullOrWhiteSpace(ingredient))
                    continue;

                // Check if this combination already exists
                if (existingIngredients.Contains(new { DrugCode = drugCode, ActiveIngredientCode = activeIngredientCode, Ingredient = ingredient }))
                    continue;

                ingredients.Add(new DrugIngredient
                {
                    DrugCode = drugCode,
                    ActiveIngredientCode = activeIngredientCode,
                    Ingredient = ingredient,
                    Strength = strength,
                    StrengthUnit = strengthUnit,
                    StrengthType = strengthType
                });

                // Add to HashSet to prevent tracking conflicts
                existingIngredients.Add(new { DrugCode = drugCode, ActiveIngredientCode = activeIngredientCode, Ingredient = ingredient });
            }

            if (ingredients.Count > 0)
            {
                await _context.DrugIngredients.AddRangeAsync(ingredients);
            }
        }



        private async Task ImportDrugCompanies(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["comp"];
            if (sheet == null) return;

            var companies = new List<DrugCompany>();

            // Fetch existing drug companies to avoid duplicates
            var existingCompanies = await _context.DrugCompanies
                .AsNoTracking()
                .Select(dc => new { dc.DrugCode, dc.CompanyCode, dc.CompanyName })
                .ToHashSetAsync();

            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                int.TryParse(sheet.Cells[row, 1].Text.Trim(), out int drugCode);
                int.TryParse(sheet.Cells[row, 3].Text.Trim(), out int companyCode);
                var companyName = sheet.Cells[row, 4].Text.Trim();
                var companyType = sheet.Cells[row, 5].Text.Trim();
                var country = sheet.Cells[row, 14].Text.Trim();
                var province = sheet.Cells[row, 13].Text.Trim();

                if (drugCode == 0 && companyCode == 0 && string.IsNullOrWhiteSpace(companyName))
                    continue;

                // Check if this combination already exists
                if (existingCompanies.Contains(new { DrugCode = drugCode, CompanyCode = companyCode, CompanyName = companyName }))
                    continue;

                companies.Add(new DrugCompany
                {
                    DrugCode = drugCode,
                    CompanyCode = companyCode,
                    CompanyName = companyName,
                    CompanyType = companyType,
                    Country = country,
                    Province = province
                });

                // Add to HashSet to prevent tracking conflicts
                existingCompanies.Add(new { DrugCode = drugCode, CompanyCode = companyCode, CompanyName = companyName });
            }

            if (companies.Count > 0)
            {
                await _context.DrugCompanies.AddRangeAsync(companies);
            }
        }


        private async Task ImportDrugStatus(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["status"];
            if (sheet == null) return;

            var statuses = new List<DrugStatus>();

            // Fetch existing drug statuses to avoid duplicates
            var existingStatuses = await _context.DrugStatuses
                .AsNoTracking()
                .Select(ds => new { ds.DrugCode, ds.Status, HistoryDate = (DateTime?)ds.HistoryDate })
                .ToHashSetAsync();

            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                int.TryParse(sheet.Cells[row, 1].Text.Trim(), out int drugCode);
                var statusText = sheet.Cells[row, 3].Text.Trim();
                DateTime? historyDate = DateTime.TryParse(sheet.Cells[row, 4].Text.Trim(), out var date) ? date : (DateTime?)null;

                if (drugCode == 0 && string.IsNullOrWhiteSpace(statusText))
                    continue;

                // Check if this combination already exists
                if (existingStatuses.Contains(new { DrugCode = drugCode, Status = statusText, HistoryDate = historyDate }))
                    continue;

                statuses.Add(new DrugStatus
                {
                    DrugCode = drugCode,
                    Status = statusText,
                    HistoryDate = historyDate
                });

                // Add to HashSet to prevent tracking conflicts
                existingStatuses.Add(new { DrugCode = drugCode, Status = statusText, HistoryDate = historyDate });
            }

            if (statuses.Count > 0)
            {
                await _context.DrugStatuses.AddRangeAsync(statuses);
            }
        }



        private async Task ImportDrugForms(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["form"];
            if (sheet == null) return;

            var forms = new List<DrugForm>();

            // Fetch existing forms to avoid duplicates
            var existingForms = await _context.DrugForms
                .AsNoTracking()
                .Select(df => new { df.DrugCode, df.PharmaceuticalForm })
                .ToHashSetAsync();

            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                int.TryParse(sheet.Cells[row, 1].Text.Trim(), out int drugCode);
                var formText = sheet.Cells[row, 3].Text.Trim();

                if (drugCode == 0 && string.IsNullOrWhiteSpace(formText))
                    continue;

                // Check if this combination already exists
                if (existingForms.Contains(new { DrugCode = drugCode, PharmaceuticalForm = formText }))
                    continue;

                forms.Add(new DrugForm
                {
                    DrugCode = drugCode,
                    PharmaceuticalForm = formText
                });

                // Add to HashSet to prevent tracking conflicts
                existingForms.Add(new { DrugCode = drugCode, PharmaceuticalForm = formText });
            }

            if (forms.Count > 0)
            {
                await _context.DrugForms.AddRangeAsync(forms);
            }
        }


        private async Task ImportDrugPharmaceuticalStds(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["pharm"];
            if (sheet == null) return;

            var pharmaceuticalStds = new List<DrugPharmaceuticalStd>();

            // Fetch existing pharmaceutical standards to avoid duplicates
            var existingPharmStds = await _context.DrugPharmaceuticalStds
                .AsNoTracking()
                .Select(dps => new { dps.DrugCode, dps.PharmaceuticalStd })
                .ToHashSetAsync();

            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                int.TryParse(sheet.Cells[row, 1].Text.Trim(), out int drugCode);
                var stdText = sheet.Cells[row, 2].Text.Trim();

                if (drugCode == 0 && string.IsNullOrWhiteSpace(stdText))
                    continue;

                // Check if this combination already exists
                if (existingPharmStds.Contains(new { DrugCode = drugCode, PharmaceuticalStd = stdText }))
                    continue;

                pharmaceuticalStds.Add(new DrugPharmaceuticalStd
                {
                    DrugCode = drugCode,
                    PharmaceuticalStd = stdText
                });

                // Add to HashSet to prevent tracking conflicts
                existingPharmStds.Add(new { DrugCode = drugCode, PharmaceuticalStd = stdText });
            }

            if (pharmaceuticalStds.Count > 0)
            {
                await _context.DrugPharmaceuticalStds.AddRangeAsync(pharmaceuticalStds);
            }
        }

    }
}
