using Drug.API.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PharmaTrack.Shared.DBModels;
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
            /*
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
            */

            await _context.SaveChangesAsync();
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
