using CsvHelper;
using CsvHelper.Configuration;
using Drug.API.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PharmaTrack.Shared.DBModels;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        public class DrugInteractionCsvModel
        {
            [CsvHelper.Configuration.Attributes.Name("DDInterID_A")]
            public string DDInterID_A { get; set; } = default!;

            [CsvHelper.Configuration.Attributes.Name("Drug_A")]
            public string Drug_A { get; set; } = default!;

            [CsvHelper.Configuration.Attributes.Name("DDInterID_B")]
            public string DDInterID_B { get; set; } = default!;

            [CsvHelper.Configuration.Attributes.Name("Drug_B")]
            public string Drug_B { get; set; } = default!;

            [CsvHelper.Configuration.Attributes.Name("Level")]
            public string Level { get; set; } = default!;
        }

        public async Task ImportDrugInteractionDataAsync()
        {
            try
            {
                // List all file paths to be processed.
                var filePaths = new[]
                {
                    "/app/DataFiles/ddinter_downloads_code_A.csv",
                    "/app/DataFiles/ddinter_downloads_code_B.csv",
                    "/app/DataFiles/ddinter_downloads_code_D.csv",
                    "/app/DataFiles/ddinter_downloads_code_H.csv",
                    "/app/DataFiles/ddinter_downloads_code_L.csv",
                    "/app/DataFiles/ddinter_downloads_code_P.csv",
                    "/app/DataFiles/ddinter_downloads_code_R.csv",
                    "/app/DataFiles/ddinter_downloads_code_V.csv",
                };

                // Preload existing hashes from the database.
                var existingHashes = await _context.DrugInteractions
                                                     .Select(di => di.Hash)
                                                     .ToListAsync();
                var hashSet = new HashSet<string>(
                    existingHashes.Where(x => x != null).Select(x => x!)
                );

                var drugInteractions = new List<DrugInteraction>();

                // Loop through each file.
                foreach (var filePath in filePaths)
                {
                    using var reader = new StreamReader(filePath);
                    using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true,
                        MissingFieldFound = null,
                        BadDataFound = null
                    });

                    // Get the records for the current file.
                    var records = csv.GetRecords<DrugInteractionCsvModel>().ToList();

                    foreach (var record in records)
                    {
                        var hashValue = GenerateHash(record);

                        // Skip if the record's hash already exists (either in the DB or added earlier).
                        if (hashSet.Contains(hashValue))
                            continue;

                        var drugInteraction = new DrugInteraction
                        {
                            DrugA = record.Drug_A,
                            DrugB = record.Drug_B,
                            DrugAID = record.DDInterID_A,
                            DrugBID = record.DDInterID_B,
                            Level = record.Level,
                            Hash = hashValue
                        };

                        drugInteractions.Add(drugInteraction);
                        hashSet.Add(hashValue); // Prevent duplicates across files.
                    }
                }

                // Add all new records in one go.
                _context.DrugInteractions.AddRange(drugInteractions);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing data: {ex.Message}");
            }
        }

        public async Task ImportDrugInfoDataAsync()
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
            var worksheet = package.Workbook.Worksheets["vet"]; // Ensure this matches the actual sheet name
            if (worksheet == null) return;

            var veterinarySpecies = new List<DrugVeterinarySpecies>();

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                try
                {
                    var vetSpecies = new DrugVeterinarySpecies
                    {
                        DrugCode = GetIntValue(worksheet.Cells[row, 1].Text),
                        VetSpecies = GetStringValue(worksheet.Cells[row, 2].Text),
                        VetSubSpecies = GetStringValue(worksheet.Cells[row, 3].Text)
                    };

                    // Generate a unique hash for this veterinary species row
                    vetSpecies.Hash = GenerateHash(vetSpecies);

                    // Check if a record with the same hash exists
                    bool exists = await _context.DrugVeterinarySpecies.AnyAsync(d => d.Hash == vetSpecies.Hash);
                    if (!exists)
                    {
                        veterinarySpecies.Add(vetSpecies);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing Drug Veterinary Species row {row}: {ex.Message}");
                }
            }

            await _context.DrugVeterinarySpecies.AddRangeAsync(veterinarySpecies);
        }

        private async Task ImportDrugTherapeuticClasses(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets["ther"]; // Ensure this matches the actual sheet name
            if (worksheet == null) return;

            var therapeuticClasses = new List<DrugTherapeuticClass>();

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                try
                {
                    var therapeuticClass = new DrugTherapeuticClass
                    {
                        DrugCode = GetIntValue(worksheet.Cells[row, 1].Text),
                        TcAtcNumber = GetStringValue(worksheet.Cells[row, 2].Text),
                        TcAtc = GetStringValue(worksheet.Cells[row, 3].Text),
                        TcAhfsNumber = GetStringValue(worksheet.Cells[row, 4].Text),
                    };

                    // Generate a unique hash for this therapeutic class row
                    therapeuticClass.Hash = GenerateHash(therapeuticClass);

                    // Check if a record with the same hash exists
                    bool exists = await _context.DrugTherapeuticClasses.AnyAsync(d => d.Hash == therapeuticClass.Hash);
                    if (!exists)
                    {
                        therapeuticClasses.Add(therapeuticClass);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing Drug Therapeutic Class row {row}: {ex.Message}");
                }
            }

            await _context.DrugTherapeuticClasses.AddRangeAsync(therapeuticClasses);
        }

        private async Task ImportDrugSchedules(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets["schedule"]; // Ensure this matches the actual sheet name
            if (worksheet == null) return;

            var schedules = new List<DrugSchedule>();

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                try
                {
                    var schedule = new DrugSchedule
                    {
                        DrugCode = GetIntValue(worksheet.Cells[row, 1].Text),
                        Schedule = GetStringValue(worksheet.Cells[row, 2].Text)
                    };

                    // Generate a unique hash for this schedule row
                    schedule.Hash = GenerateHash(schedule);

                    // Check if a record with the same hash exists
                    bool exists = await _context.DrugSchedules.AnyAsync(d => d.Hash == schedule.Hash);
                    if (!exists)
                    {
                        schedules.Add(schedule);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing Drug Schedule row {row}: {ex.Message}");
                }
            }

            await _context.DrugSchedules.AddRangeAsync(schedules);
        }

        private async Task ImportDrugRoutes(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets["route"]; // Ensure this matches the actual sheet name
            if (worksheet == null) return;

            var routes = new List<DrugRoute>();

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                try
                {
                    var route = new DrugRoute
                    {
                        DrugCode = GetIntValue(worksheet.Cells[row, 1].Text),
                        RouteOfAdministrationCode = GetIntValue(worksheet.Cells[row, 2].Text),
                        RouteOfAdministration = GetStringValue(worksheet.Cells[row, 3].Text)
                    };

                    // Generate a unique hash for this route row
                    route.Hash = GenerateHash(route);

                    // Check if a record with the same hash exists
                    bool exists = await _context.DrugRoutes.AnyAsync(d => d.Hash == route.Hash);
                    if (!exists)
                    {
                        routes.Add(route);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing Drug Route row {row}: {ex.Message}");
                }
            }

            await _context.DrugRoutes.AddRangeAsync(routes);
        }

        private async Task ImportDrugPharmaceuticalStds(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets["pharm"]; // Ensure this matches the actual sheet name
            if (worksheet == null) return;

            var pharmaceuticalStds = new List<DrugPharmaceuticalStd>();

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                try
                {
                    var pharmStd = new DrugPharmaceuticalStd
                    {
                        DrugCode = GetIntValue(worksheet.Cells[row, 1].Text),
                        PharmaceuticalStd = GetStringValue(worksheet.Cells[row, 2].Text)
                    };

                    // Generate a unique hash for this pharmaceutical standard row
                    pharmStd.Hash = GenerateHash(pharmStd);

                    // Check if a record with the same hash exists
                    bool exists = await _context.DrugPharmaceuticalStds.AnyAsync(d => d.Hash == pharmStd.Hash);
                    if (!exists)
                    {
                        pharmaceuticalStds.Add(pharmStd);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing Drug Pharmaceutical Std row {row}: {ex.Message}");
                }
            }

            await _context.DrugPharmaceuticalStds.AddRangeAsync(pharmaceuticalStds);
        }

        private async Task ImportDrugPackaging(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets["package"]; // Ensure this matches the actual sheet name
            if (worksheet == null) return;

            var packagings = new List<DrugPackaging>();

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                try
                {
                    var packaging = new DrugPackaging
                    {
                        DrugCode = GetIntValue(worksheet.Cells[row, 1].Text),
                       
                        Upc = GetStringValue(worksheet.Cells[row, 2].Text),
                        PackageSizeUnit = GetStringValue(worksheet.Cells[row, 3].Text),
                        PackageType = GetStringValue(worksheet.Cells[row, 4].Text),
                        PackageSize = GetStringValue(worksheet.Cells[row, 5].Text),
                        ProductInformation = GetStringValue(worksheet.Cells[row, 6].Text)
                    };

                    // Generate a unique hash for this packaging row
                    packaging.Hash = GenerateHash(packaging);

                    // Check if a record with the same hash exists
                    bool exists = await _context.DrugPackagings.AnyAsync(d => d.Hash == packaging.Hash);
                    if (!exists)
                    {
                        packagings.Add(packaging);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing Drug Packaging row {row}: {ex.Message}");
                }
            }

            await _context.DrugPackagings.AddRangeAsync(packagings);
        }

        private async Task ImportDrugForms(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets["form"]; // Ensure this matches the actual sheet name
            if (worksheet == null) return;

            var forms = new List<DrugForm>();

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                try
                {
                    var form = new DrugForm
                    {
                        DrugCode = GetIntValue(worksheet.Cells[row, 1].Text),
                        PharmFormCode = GetIntValue(worksheet.Cells[row, 2].Text),
                        PharmaceuticalForm = GetStringValue(worksheet.Cells[row, 3].Text)
                    };

                    // Generate a unique hash for this form row
                    form.Hash = GenerateHash(form);

                    // Check if a record with the same hash exists
                    bool exists = await _context.DrugForms.AnyAsync(d => d.Hash == form.Hash);
                    if (!exists)
                    {
                        forms.Add(form);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing Drug Form row {row}: {ex.Message}");
                }
            }

            await _context.DrugForms.AddRangeAsync(forms);
        }

        private async Task ImportDrugStatus(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets["status"]; // Ensure this matches the actual sheet name
            if (worksheet == null) return;

            var statuses = new List<DrugStatus>();

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                try
                {
                    var status = new DrugStatus
                    {
                        DrugCode = GetIntValue(worksheet.Cells[row, 1].Text),
                        CurrentStatusFlag = GetStringValue(worksheet.Cells[row, 2].Text),
                        Status = GetStringValue(worksheet.Cells[row, 3].Text),
                        HistoryDate = GetDateValue(worksheet.Cells[row, 4].Text)
                    };

                    // Generate a unique hash for this status row
                    status.Hash = GenerateHash(status);

                    // Check if a record with the same hash exists
                    bool exists = await _context.DrugStatuses.AnyAsync(d => d.Hash == status.Hash);
                    if (!exists)
                    {
                        statuses.Add(status);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing Drug Status row {row}: {ex.Message}");
                }
            }

            await _context.DrugStatuses.AddRangeAsync(statuses);
        }

        private async Task ImportDrugCompanies(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets["comp"]; // Ensure this matches the actual sheet name
            if (worksheet == null) return;

            var companies = new List<DrugCompany>();

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                try
                {
                    var company = new DrugCompany
                    {
                        DrugCode = GetIntValue(worksheet.Cells[row, 1].Text),
                        MfrCode = GetStringValue(worksheet.Cells[row, 2].Text),
                        CompanyCode = GetIntValue(worksheet.Cells[row, 3].Text),
                        CompanyName = GetStringValue(worksheet.Cells[row, 4].Text),
                        CompanyType = GetStringValue(worksheet.Cells[row, 5].Text),
                        AddressMailingFlag = GetStringValue(worksheet.Cells[row, 6].Text),
                        AddressBillingFlag = GetStringValue(worksheet.Cells[row, 7].Text),
                        AddressNotificationFlag = GetStringValue(worksheet.Cells[row, 8].Text),
                        AddressOther = GetStringValue(worksheet.Cells[row, 9].Text),
                        SuiteNumber = GetStringValue(worksheet.Cells[row, 10].Text),
                        StreetName = GetStringValue(worksheet.Cells[row, 11].Text),
                        CityName = GetStringValue(worksheet.Cells[row, 12].Text),
                        Province = GetStringValue(worksheet.Cells[row, 13].Text),
                        Country = GetStringValue(worksheet.Cells[row, 14].Text),
                        PostalCode = GetStringValue(worksheet.Cells[row, 15].Text),
                        PostOfficeBox = GetStringValue(worksheet.Cells[row, 16].Text)
                    };

                    // Generate a unique hash for this company row
                    company.Hash = GenerateHash(company);

                    // Check if a record with the same hash exists
                    bool exists = await _context.DrugCompanies.AnyAsync(d => d.Hash == company.Hash);
                    if (!exists)
                    {
                        companies.Add(company);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing Drug Company row {row}: {ex.Message}");
                }
            }

            await _context.DrugCompanies.AddRangeAsync(companies);
        }

        private async Task ImportDrugIngredients(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets["ingred"]; // Ensure this matches the actual sheet name
            if (worksheet == null) return;

            var ingredients = new List<DrugIngredient>();

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                try
                {
                    var ingredient = new DrugIngredient
                    {
                        DrugCode = GetIntValue(worksheet.Cells[row, 1].Text),
                        ActiveIngredientCode = GetIntValue(worksheet.Cells[row, 2].Text),
                        Ingredient = GetStringValue(worksheet.Cells[row, 3].Text),
                        IngredientSuppliedInd = GetStringValue(worksheet.Cells[row, 4].Text),
                        Strength = GetStringValue(worksheet.Cells[row, 5].Text),
                        StrengthUnit = GetStringValue(worksheet.Cells[row, 6].Text),
                        StrengthType = GetStringValue(worksheet.Cells[row, 7].Text),
                        DosageValue = GetStringValue(worksheet.Cells[row, 8].Text),
                        Base = GetStringValue(worksheet.Cells[row, 9].Text),
                        DosageUnit = GetStringValue(worksheet.Cells[row, 10].Text),
                        Notes = GetStringValue(worksheet.Cells[row, 11].Text)
                    };
                    
                    // Generate a unique hash for this ingredient row
                    ingredient.Hash = GenerateHash(ingredient);

                    // Check if a record with the same hash exists
                    bool exists = await _context.DrugIngredients.AnyAsync(d => d.Hash == ingredient.Hash);
                    if (!exists)
                    {
                        ingredients.Add(ingredient);
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing Drug Ingredient row {row}: {ex.Message}");
                }
            }

            await _context.DrugIngredients.AddRangeAsync(ingredients);
        }

        private async Task ImportDrugs(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets["drug"];
            if (worksheet == null) return;

            var drugs = new List<DrugProduct>();

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                try
                {
                    var drug = new DrugProduct
                    {
                        DrugCode = GetIntValue(worksheet.Cells[row, 1].Text),
                        ProductCategorization = GetStringValue(worksheet.Cells[row, 2].Text),
                        Class = GetStringValue(worksheet.Cells[row, 3].Text),
                        DrugIdentificationNumber = GetStringValue(worksheet.Cells[row, 4].Text),
                        BrandName = GetStringValue(worksheet.Cells[row, 5].Text),
                        Descriptor = GetStringValue(worksheet.Cells[row, 6].Text),
                        PediatricFlag = GetStringValue(worksheet.Cells[row, 7].Text),
                        AccessionNumber = GetStringValue(worksheet.Cells[row, 8].Text),
                        NumberOfAis = GetStringValue(worksheet.Cells[row, 9].Text),
                        LastUpdateDate = GetDateValue(worksheet.Cells[row, 10].Text),
                        AiGroupNo = GetStringValue(worksheet.Cells[row, 11].Text)
                    };

                    // Generate a unique hash for this row
                    drug.Hash = GenerateHash(drug);

                    // Check if a record with the same hash exists
                    bool exists = await _context.Drugs.AnyAsync(d => d.Hash == drug.Hash);
                    if (!exists)
                    {
                        drugs.Add(drug);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing Drug row {row}: {ex.Message}");
                }
            }

            await _context.Drugs.AddRangeAsync(drugs);
        }

        private static int GetIntValue(string value)
        {
            return int.TryParse(value, out int result) ? result : 0;
        }

        private static string? GetStringValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static DateTime? GetDateValue(string value)
        {
            return DateTime.TryParse(value, out DateTime result) ? result : (DateTime?)null;
        }

        private static string GenerateHash<T>(T obj)
        {

            // Convert the object into a JSON string (ignoring null properties for consistency)
            string json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
            return Convert.ToHexStringLower(bytes);
        }
    }
}
