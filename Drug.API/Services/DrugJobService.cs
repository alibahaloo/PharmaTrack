using CsvHelper;
using CsvHelper.Configuration;
using Drug.API.Data;
using Microsoft.EntityFrameworkCore;
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
                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine($"File not found: {filePath}");
                        continue;
                    }

                    using var reader = new StreamReader(filePath);
                    using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = false,
                        HeaderValidated = null,
                        MissingFieldFound = null,
                        BadDataFound = null
                    });

                    // Get the records for the current file mapped directly to our DrugInteraction entity.
                    var records = csv.GetRecords<DrugInteraction>().ToList();
                    int row = 1; // Header is row 1.
                    foreach (var record in records)
                    {
                        row++;
                        try
                        {
                            // Generate a unique hash for this record.
                            record.Hash = GenerateHash(record);

                            // Skip if a record with the same hash already exists.
                            if (hashSet.Contains(record.Hash))
                                continue;

                            drugInteractions.Add(record);
                            hashSet.Add(record.Hash); // Prevent duplicates across files.
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error importing Drug Interaction row {row} from {filePath}: {ex.Message}");
                        }
                    }
                }

                // Add all new records in one go.
                if (drugInteractions.Count != 0)
                {
                    _context.DrugInteractions.AddRange(drugInteractions);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing data: {ex.Message}");
            }
        }
        public async Task ImportDrugInfoDataAsync()
        {
            await ImportDrugs();
            await ImportDrugIngredients();
            await ImportDrugCompanies();
            await ImportDrugStatus();
            await ImportDrugForms();
            await ImportDrugPackaging();
            await ImportDrugPharmaceuticalStds();
            await ImportDrugRoutes();
            await ImportDrugSchedules();
            await ImportDrugTherapeuticClasses();
            await ImportDrugVeterinarySpecies();

            await _context.SaveChangesAsync();
        }
        private async Task ImportDrugVeterinarySpecies()
        {
            string filePath = "/app/DataFiles/vet.txt";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Text file not found at {filePath}");
            }

            // Preload existing veterinary species hashes from the database.
            var existingHashes = await _context.DrugVeterinarySpecies
                                                 .Select(dvs => dvs.Hash)
                                                 .ToListAsync();
            var hashSet = new HashSet<string>(
                existingHashes.Where(x => x != null).Select(x => x!)
            );

            var veterinarySpecies = new List<DrugVeterinarySpecies>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null
            }))
            {
                var records = csv.GetRecords<DrugVeterinarySpecies>().ToList();
                int row = 1; // Header is row 1.
                foreach (var record in records)
                {
                    row++;
                    try
                    {
                        // Generate a unique hash for this record.
                        record.Hash = GenerateHash(record);

                        // Skip if a record with the same hash already exists.
                        if (hashSet.Contains(record.Hash))
                            continue;

                        veterinarySpecies.Add(record);
                        hashSet.Add(record.Hash); // Prevent duplicates.
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error importing Drug Veterinary Species row {row}: {ex.Message}");
                    }
                }
            }

            if (veterinarySpecies.Count != 0)
            {
                await _context.DrugVeterinarySpecies.AddRangeAsync(veterinarySpecies);
            }
        }
        private async Task ImportDrugTherapeuticClasses()
        {
            string filePath = "/app/DataFiles/ther.txt";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Text file not found at {filePath}");
            }

            // Preload existing therapeutic class hashes from the database.
            var existingHashes = await _context.DrugTherapeuticClasses
                                                 .Select(dt => dt.Hash)
                                                 .ToListAsync();
            var hashSet = new HashSet<string>(
                existingHashes.Where(x => x != null).Select(x => x!)
            );

            var therapeuticClasses = new List<DrugTherapeuticClass>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null
            }))
            {
                var records = csv.GetRecords<DrugTherapeuticClass>().ToList();
                int row = 1; // Header is row 1.
                foreach (var record in records)
                {
                    row++;
                    try
                    {
                        // Generate a unique hash for this therapeutic class record.
                        record.Hash = GenerateHash(record);

                        // Skip if a record with the same hash already exists.
                        if (hashSet.Contains(record.Hash))
                            continue;

                        therapeuticClasses.Add(record);
                        hashSet.Add(record.Hash); // Prevent duplicates.
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error importing Drug Therapeutic Class row {row}: {ex.Message}");
                    }
                }
            }

            if (therapeuticClasses.Count != 0)
            {
                await _context.DrugTherapeuticClasses.AddRangeAsync(therapeuticClasses);
            }
        }
        private async Task ImportDrugSchedules()
        {
            string filePath = "/app/DataFiles/schedule.txt";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Text file not found at {filePath}");
            }

            // Preload existing schedule hashes from the database.
            var existingHashes = await _context.DrugSchedules
                                                 .Select(ds => ds.Hash)
                                                 .ToListAsync();
            var hashSet = new HashSet<string>(
                existingHashes.Where(x => x != null).Select(x => x!)
            );

            var schedules = new List<DrugSchedule>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null
            }))
            {
                var records = csv.GetRecords<DrugSchedule>().ToList();
                int row = 1; // header is row 1.
                foreach (var record in records)
                {
                    row++;
                    try
                    {
                        // Generate a unique hash for this schedule record.
                        record.Hash = GenerateHash(record);

                        // Skip if a record with the same hash already exists.
                        if (hashSet.Contains(record.Hash))
                            continue;

                        schedules.Add(record);
                        hashSet.Add(record.Hash); // Prevent duplicates.
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error importing Drug Schedule row {row}: {ex.Message}");
                    }
                }
            }

            if (schedules.Count != 0)
            {
                await _context.DrugSchedules.AddRangeAsync(schedules);
            }
        }
        private async Task ImportDrugRoutes()
        {
            string filePath = "/app/DataFiles/route.txt";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Text file not found at {filePath}");
            }

            // Preload existing route hashes from the database, filtering out any null values.
            var existingHashes = await _context.DrugRoutes
                                                 .Select(dr => dr.Hash)
                                                 .ToListAsync();
            var hashSet = new HashSet<string>(
                existingHashes.Where(x => x != null).Select(x => x!)
            );

            var routes = new List<DrugRoute>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null
            }))
            {
                var records = csv.GetRecords<DrugRoute>().ToList();
                int row = 1; // Header is row 1.
                foreach (var record in records)
                {
                    row++;
                    try
                    {
                        // Generate a unique hash for this route record.
                        record.Hash = GenerateHash(record);

                        // Skip this record if a record with the same hash already exists.
                        if (hashSet.Contains(record.Hash))
                            continue;

                        routes.Add(record);
                        hashSet.Add(record.Hash); // Prevent duplicates.
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error importing Drug Route row {row}: {ex.Message}");
                    }
                }
            }

            if (routes.Count != 0)
            {
                await _context.DrugRoutes.AddRangeAsync(routes);
            }
        }
        private async Task ImportDrugPharmaceuticalStds()
        {
            string filePath = "/app/DataFiles/pharm.txt";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Text file not found at {filePath}");
            }

            // Preload existing pharmaceutical standard hashes from the database.
            var existingHashes = await _context.DrugPharmaceuticalStds
                                                 .Select(dps => dps.Hash)
                                                 .ToListAsync();
            var hashSet = new HashSet<string>(
                existingHashes.Where(x => x != null).Select(x => x!)
            );

            var pharmaceuticalStds = new List<DrugPharmaceuticalStd>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null
            }))
            {
                var records = csv.GetRecords<DrugPharmaceuticalStd>().ToList();
                int row = 1; // Header is row 1.
                foreach (var record in records)
                {
                    row++;
                    try
                    {
                        // Generate a unique hash for this record.
                        record.Hash = GenerateHash(record);

                        // Skip if a record with the same hash already exists.
                        if (hashSet.Contains(record.Hash))
                            continue;

                        pharmaceuticalStds.Add(record);
                        hashSet.Add(record.Hash);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error importing Drug Pharmaceutical Std row {row}: {ex.Message}");
                    }
                }
            }

            if (pharmaceuticalStds.Count != 0)
            {
                await _context.DrugPharmaceuticalStds.AddRangeAsync(pharmaceuticalStds);
            }
        }
        private async Task ImportDrugPackaging()
        {
            string filePath = "/app/DataFiles/package.txt";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Text file not found at {filePath}");
            }

            // Preload existing packaging hashes from the database.
            var existingHashes = await _context.DrugPackagings
                                                 .Select(dp => dp.Hash)
                                                 .ToListAsync();
            var hashSet = new HashSet<string>(
                existingHashes.Where(x => x != null).Select(x => x!)
            );

            var packagings = new List<DrugPackaging>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null
            }))
            {
                var records = csv.GetRecords<DrugPackaging>().ToList();
                int row = 1; // header is row 1
                foreach (var record in records)
                {
                    row++;
                    try
                    {
                        // Generate a unique hash for this packaging record.
                        record.Hash = GenerateHash(record);

                        // Skip if a record with the same hash already exists.
                        if (hashSet.Contains(record.Hash))
                            continue;

                        packagings.Add(record);
                        hashSet.Add(record.Hash); // Prevent duplicates.
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error importing Drug Packaging row {row}: {ex.Message}");
                    }
                }
            }

            if (packagings.Count != 0)
            {
                await _context.DrugPackagings.AddRangeAsync(packagings);
            }
        }
        private async Task ImportDrugForms()
        {
            string filePath = "/app/DataFiles/form.txt";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Text file not found at {filePath}");
            }

            // Preload existing form hashes from the database.
            var existingHashes = await _context.DrugForms
                                                 .Select(df => df.Hash)
                                                 .ToListAsync();
            var hashSet = new HashSet<string>(
                existingHashes.Where(x => x != null).Select(x => x!)
            );

            var forms = new List<DrugForm>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null
            }))
            {
                var records = csv.GetRecords<DrugForm>().ToList();
                int row = 1; // header is row 1
                foreach (var record in records)
                {
                    row++;
                    try
                    {
                        // Generate a unique hash for this form record.
                        record.Hash = GenerateHash(record);

                        // Skip if a record with the same hash already exists.
                        if (hashSet.Contains(record.Hash))
                            continue;

                        forms.Add(record);
                        hashSet.Add(record.Hash); // Prevent duplicates.
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error importing Drug Form row {row}: {ex.Message}");
                    }
                }
            }

            if (forms.Count != 0)
            {
                await _context.DrugForms.AddRangeAsync(forms);
            }
        }
        private async Task ImportDrugStatus()
        {
            string filePath = "/app/DataFiles/status.txt";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Text file not found at {filePath}");
            }

            // Preload existing status hashes from the database.
            var existingHashes = await _context.DrugStatuses
                                                 .Select(ds => ds.Hash)
                                                 .ToListAsync();
            var hashSet = new HashSet<string>(
                existingHashes.Where(x => x != null).Select(x => x!)
            );

            var statuses = new List<DrugStatus>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null
            }))
            {
                // CsvHelper automatically maps CSV rows to DrugStatus instances.
                var records = csv.GetRecords<DrugStatus>().ToList();
                int row = 1; // Assume header is row 1.
                foreach (var record in records)
                {
                    row++;
                    try
                    {
                        // Generate a unique hash for this status record.
                        record.Hash = GenerateHash(record);

                        // Skip if a record with the same hash exists.
                        if (hashSet.Contains(record.Hash))
                            continue;

                        statuses.Add(record);
                        hashSet.Add(record.Hash); // Prevent duplicates.
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error importing Drug Status row {row}: {ex.Message}");
                    }
                }
            }

            if (statuses.Count != 0)
            {
                await _context.DrugStatuses.AddRangeAsync(statuses);
            }
        }
        private async Task ImportDrugCompanies()
        {
            string filePath = "/app/DataFiles/comp.txt";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Text file not found at {filePath}");
            }

            // Preload existing company hashes from the database.
            var existingHashes = await _context.DrugCompanies
                                                 .Select(dc => dc.Hash)
                                                 .ToListAsync();
            var hashSet = new HashSet<string>(
                existingHashes.Where(x => x != null).Select(x => x!)
            );

            var companies = new List<DrugCompany>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null
            }))
            {
                // Get all records from the CSV mapped directly into DrugCompany instances.
                var records = csv.GetRecords<DrugCompany>().ToList();
                int row = 1; // Header is row 1
                foreach (var record in records)
                {
                    row++;
                    try
                    {
                        // Generate a unique hash for this company record.
                        record.Hash = GenerateHash(record);

                        // Skip the record if a record with the same hash already exists.
                        if (hashSet.Contains(record.Hash))
                            continue;

                        companies.Add(record);
                        hashSet.Add(record.Hash); // Prevent duplicates across files.
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error importing Drug Company row {row}: {ex.Message}");
                    }
                }
            }

            if (companies.Count != 0)
            {
                await _context.DrugCompanies.AddRangeAsync(companies);
            }
        }
        private async Task ImportDrugIngredients()
        {
            string filePath = "/app/DataFiles/ingred.txt";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Text file not found at {filePath}");
            }

            // Preload existing ingredient hashes from the database.
            var existingHashes = await _context.DrugIngredients
                                                 .Select(di => di.Hash)
                                                 .ToListAsync();
            var hashSet = new HashSet<string>(
                existingHashes.Where(x => x != null).Select(x => x!)
            );

            var ingredients = new List<DrugIngredient>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null
            }))
            {
                // CsvHelper will use your DrugIngredient class directly.
                var records = csv.GetRecords<DrugIngredient>().ToList();

                int row = 1; // assume header is row 1.
                foreach (var record in records)
                {
                    row++;
                    try
                    {
                        // Generate a unique hash for this ingredient row.
                        record.Hash = GenerateHash(record);

                        // Skip if a record with the same hash already exists.
                        if (hashSet.Contains(record.Hash))
                            continue;

                        ingredients.Add(record);
                        hashSet.Add(record.Hash); // Prevent duplicates.
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error importing Drug Ingredient row {row}: {ex.Message}");
                    }
                }
            }

            if (ingredients.Count != 0)
            {
                await _context.DrugIngredients.AddRangeAsync(ingredients);
            }
        }
        private async Task ImportDrugs()
        {
            string filePath = "/app/DataFiles/drug.txt";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Text file not found at {filePath}");
            }

            // Preload existing drug hashes from the database.
            var existingHashes = await _context.Drugs
                                                 .Select(d => d.Hash)
                                                 .ToListAsync();
            var hashSet = new HashSet<string>(
                existingHashes.Where(x => x != null).Select(x => x!)
            );

            var drugs = new List<DrugProduct>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null
            }))
            {
                // CsvHelper will create instances of DrugProduct using the CSV headers
                var records = csv.GetRecords<DrugProduct>().ToList();

                int row = 1; // Header is row 1.
                foreach (var record in records)
                {
                    row++;
                    try
                    {
                        // Generate a unique hash for the record.
                        record.Hash = GenerateHash(record);

                        // Skip if this record already exists (in the DB or in this batch).
                        if (hashSet.Contains(record.Hash))
                            continue;

                        drugs.Add(record);
                        hashSet.Add(record.Hash); // Prevent duplicates.
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error importing Drug row {row}: {ex.Message}");
                    }
                }
            }

            if (drugs.Count != 0)
            {
                await _context.Drugs.AddRangeAsync(drugs);
            }
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
