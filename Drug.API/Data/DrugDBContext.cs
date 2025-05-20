using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Drug.API.Data
{
    public class DrugDBContext : DbContext
    {
        public DrugDBContext(DbContextOptions<DrugDBContext> options) : base(options) { }

        // DbSets for all tables
        public DbSet<DrugProduct> DrugProducts { get; set; } = null!;
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

    public class DrugProduct
    {
        [Key]
        [Ignore] // Not imported from CSV.
        public int Id { get; set; }

        [Column("HASH")]
        [Ignore] // Generated in code.
        public string? Hash { get; set; }

        [Column("DRUG_CODE")]
        [CsvHelper.Configuration.Attributes.Index(0)]
        public int DrugCode { get; set; }

        [Column("PRODUCT_CATEGORIZATION")]
        [CsvHelper.Configuration.Attributes.Index(1)]
        public string? ProductCategorization { get; set; }

        [Column("CLASS")]
        [CsvHelper.Configuration.Attributes.Index(2)]
        public string? Class { get; set; }

        [Column("DRUG_IDENTIFICATION_NUMBER")]
        [CsvHelper.Configuration.Attributes.Index(3)]
        public string? DrugIdentificationNumber { get; set; }

        [Column("BRAND_NAME")]
        [CsvHelper.Configuration.Attributes.Index(4)]
        public string? BrandName { get; set; }

        [Column("DESCRIPTOR")]
        [CsvHelper.Configuration.Attributes.Index(5)]
        public string? Descriptor { get; set; }

        [Column("PEDIATRIC_FLAG")]
        [CsvHelper.Configuration.Attributes.Index(6)]
        public string? PediatricFlag { get; set; }

        [Column("ACCESSION_NUMBER")]
        [CsvHelper.Configuration.Attributes.Index(7)]
        public string? AccessionNumber { get; set; }

        [Column("NUMBER_OF_AIS")]
        [CsvHelper.Configuration.Attributes.Index(8)]
        public string? NumberOfAis { get; set; }

        [Column("LAST_UPDATE_DATE")]
        [CsvHelper.Configuration.Attributes.Index(9)]
        public DateTime? LastUpdateDate { get; set; }

        [Column("AI_GROUP_NO")]
        [CsvHelper.Configuration.Attributes.Index(10)]
        public string? AiGroupNo { get; set; }
    }
    public class DrugIngredient
    {
        [Key]
        [Ignore] // Not present in CSV.
        public int Id { get; set; }

        [Column("HASH")]
        [Ignore] // We'll generate this later; not read from CSV.
        public string? Hash { get; set; }

        [Column("DRUG_CODE")]
        [CsvHelper.Configuration.Attributes.Index(0)]
        [Required]
        public int DrugCode { get; set; }

        [Column("ACTIVE_INGREDIENT_CODE")]
        [CsvHelper.Configuration.Attributes.Index(1)]
        public int ActiveIngredientCode { get; set; }

        [Column("INGREDIENT")]
        [CsvHelper.Configuration.Attributes.Index(2)]
        public string? Ingredient { get; set; }

        [Column("INGREDIENT_SUPPLIED_IND")]
        [CsvHelper.Configuration.Attributes.Index(3)]
        public string? IngredientSuppliedInd { get; set; }

        [Column("STRENGTH")]
        [CsvHelper.Configuration.Attributes.Index(4)]
        public string? Strength { get; set; }

        [Column("STRENGTH_UNIT")]
        [CsvHelper.Configuration.Attributes.Index(5)]
        public string? StrengthUnit { get; set; }

        [Column("STRENGTH_TYPE")]
        [CsvHelper.Configuration.Attributes.Index(6)]
        public string? StrengthType { get; set; }

        [Column("DOSAGE_VALUE")]
        [CsvHelper.Configuration.Attributes.Index(7)]
        public string? DosageValue { get; set; }

        [Column("BASE")]
        [CsvHelper.Configuration.Attributes.Index(8)]
        public string? Base { get; set; }

        [Column("DOSAGE_UNIT")]
        [CsvHelper.Configuration.Attributes.Index(9)]
        public string? DosageUnit { get; set; }

        [Column("NOTES")]
        [CsvHelper.Configuration.Attributes.Index(10)]
        public string? Notes { get; set; }
    }
    public class DrugCompany
    {
        [Key]
        [Ignore] // Not present in CSV.
        public int Id { get; set; }

        [Column("HASH")]
        [Ignore] // Generated in code; not imported from CSV.
        public string? Hash { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        [CsvHelper.Configuration.Attributes.Index(0)]
        public int DrugCode { get; set; }

        [Column("MFR_CODE")]
        [CsvHelper.Configuration.Attributes.Index(1)]
        public string? MfrCode { get; set; }

        [Column("COMPANY_CODE")]
        [CsvHelper.Configuration.Attributes.Index(2)]
        public int CompanyCode { get; set; }

        [Column("COMPANY_NAME")]
        [CsvHelper.Configuration.Attributes.Index(3)]
        public string? CompanyName { get; set; }

        [Column("COMPANY_TYPE")]
        [CsvHelper.Configuration.Attributes.Index(4)]
        public string? CompanyType { get; set; }

        [Column("ADDRESS_MAILING_FLAG")]
        [CsvHelper.Configuration.Attributes.Index(5)]
        public string? AddressMailingFlag { get; set; }

        [Column("ADDRESS_BILLING_FLAG")]
        [CsvHelper.Configuration.Attributes.Index(6)]
        public string? AddressBillingFlag { get; set; }

        [Column("ADDRESS_NOTIFICATION_FLAG")]
        [CsvHelper.Configuration.Attributes.Index(7)]
        public string? AddressNotificationFlag { get; set; }

        [Column("ADDRESS_OTHER")]
        [CsvHelper.Configuration.Attributes.Index(8)]
        public string? AddressOther { get; set; }

        [Column("SUITE_NUMBER")]
        [CsvHelper.Configuration.Attributes.Index(9)]
        public string? SuiteNumber { get; set; }

        [Column("STREET_NAME")]
        [CsvHelper.Configuration.Attributes.Index(10)]
        public string? StreetName { get; set; }

        [Column("CITY_NAME")]
        [CsvHelper.Configuration.Attributes.Index(11)]
        public string? CityName { get; set; }

        [Column("PROVINCE")]
        [CsvHelper.Configuration.Attributes.Index(12)]
        public string? Province { get; set; }

        [Column("COUNTRY")]
        [CsvHelper.Configuration.Attributes.Index(13)]
        public string? Country { get; set; }

        [Column("POSTAL_CODE")]
        [CsvHelper.Configuration.Attributes.Index(14)]
        public string? PostalCode { get; set; }

        [Column("POST_OFFICE_BOX")]
        [CsvHelper.Configuration.Attributes.Index(15)]
        public string? PostOfficeBox { get; set; }
    }
    public class DrugStatus
    {
        [Key]
        [Ignore] // Not in CSV file.
        public int Id { get; set; }

        [Column("HASH")]
        [Ignore] // We'll generate this later.
        public string? Hash { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        [CsvHelper.Configuration.Attributes.Index(0)]
        public int DrugCode { get; set; }

        [Column("CURRENT_STATUS_FLAG")]
        [CsvHelper.Configuration.Attributes.Index(1)]
        public string? CurrentStatusFlag { get; set; }

        [Column("STATUS")]
        [CsvHelper.Configuration.Attributes.Index(2)]
        public string? Status { get; set; }

        [Column("HISTORY_DATE")]
        [CsvHelper.Configuration.Attributes.Index(3)]
        public DateTime? HistoryDate { get; set; }
    }
    public class DrugForm
    {
        [Key]
        [Ignore]
        public int Id { get; set; }

        [Column("HASH")]
        [Ignore] // Generated in code; not imported from CSV.
        public string? Hash { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        [CsvHelper.Configuration.Attributes.Index(0)]
        public int DrugCode { get; set; }

        [Column("PHARM_FORM_CODE")]
        [CsvHelper.Configuration.Attributes.Index(1)]
        public int PharmFormCode { get; set; }

        [Column("PHARMACEUTICAL_FORM")]
        [CsvHelper.Configuration.Attributes.Index(2)]
        public string? PharmaceuticalForm { get; set; }
    }
    public class DrugPackaging
    {
        [Key]
        [Ignore]
        public int Id { get; set; }

        [Column("HASH")]
        [Ignore] // Generated in code; not imported from CSV.
        public string? Hash { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        [CsvHelper.Configuration.Attributes.Index(0)]
        public int DrugCode { get; set; }

        [Column("UPC")]
        [CsvHelper.Configuration.Attributes.Index(1)]
        public string? Upc { get; set; }

        [Column("PACKAGE_SIZE_UNIT")]
        [CsvHelper.Configuration.Attributes.Index(2)]
        public string? PackageSizeUnit { get; set; }

        [Column("PACKAGE_TYPE")]
        [CsvHelper.Configuration.Attributes.Index(3)]
        public string? PackageType { get; set; }

        [Column("PACKAGE_SIZE")]
        [CsvHelper.Configuration.Attributes.Index(4)]
        public string? PackageSize { get; set; }

        [Column("PRODUCT_INFORMATION")]
        [CsvHelper.Configuration.Attributes.Index(5)]
        public string? ProductInformation { get; set; }
    }
    public class DrugPharmaceuticalStd
    {
        [Key]
        [Ignore]
        public int Id { get; set; }

        [Column("HASH")]
        [Ignore] // Generated in code; not imported from CSV.
        public string? Hash { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        [CsvHelper.Configuration.Attributes.Index(0)]
        public int DrugCode { get; set; }

        [Column("PHARMACEUTICAL_STD")]
        [CsvHelper.Configuration.Attributes.Index(1)]
        public string? PharmaceuticalStd { get; set; }
    }
    public class DrugRoute
    {
        [Key]
        [Ignore] // Not imported from CSV.
        public int Id { get; set; }

        [Column("HASH")]
        [Ignore] // Generated in code.
        public string? Hash { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        [CsvHelper.Configuration.Attributes.Index(0)]
        public int DrugCode { get; set; }

        [Column("ROUTE_OF_ADMINISTRATION_CODE")]
        [Required]
        [CsvHelper.Configuration.Attributes.Index(1)]
        public int RouteOfAdministrationCode { get; set; }

        [Column("ROUTE_OF_ADMINISTRATION")]
        [CsvHelper.Configuration.Attributes.Index(2)]
        public string? RouteOfAdministration { get; set; }
    }
    public class DrugSchedule
    {
        [Key]
        [Ignore] // Not imported from CSV.
        public int Id { get; set; }

        [Column("HASH")]
        [Ignore] // Generated in code.
        public string? Hash { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        [CsvHelper.Configuration.Attributes.Index(0)]
        public int DrugCode { get; set; }

        [Column("SCHEDULE")]
        [CsvHelper.Configuration.Attributes.Index(1)]
        public string? Schedule { get; set; }
    }
    public class DrugTherapeuticClass
    {
        [Key]
        [Ignore] // Not imported from CSV.
        public int Id { get; set; }

        [Column("HASH")]
        [Ignore] // Generated in code.
        public string? Hash { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        [CsvHelper.Configuration.Attributes.Index(0)]
        public int DrugCode { get; set; }

        [Column("TC_ATC_NUMBER")]
        [CsvHelper.Configuration.Attributes.Index(1)]
        public string? TcAtcNumber { get; set; }

        [Column("TC_ATC")]
        [CsvHelper.Configuration.Attributes.Index(2)]
        public string? TcAtc { get; set; }

        [Column("TC_AHFS_NUMBER")]
        [CsvHelper.Configuration.Attributes.Index(3)]
        public string? TcAhfsNumber { get; set; }

        [Column("TC_AHFS")]
        [CsvHelper.Configuration.Attributes.Index(4)]
        public string? TcAhfs { get; set; }
    }
    public class DrugVeterinarySpecies
    {
        [Key]
        [Ignore] // Not imported from CSV.
        public int Id { get; set; }

        [Column("HASH")]
        [Ignore] // Generated in code.
        public string? Hash { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        [CsvHelper.Configuration.Attributes.Index(0)]
        public int DrugCode { get; set; }

        [Column("VET_SPECIES")]
        [CsvHelper.Configuration.Attributes.Index(1)]
        public string? VetSpecies { get; set; }

        [Column("VET_SUB_SPECIES")]
        [CsvHelper.Configuration.Attributes.Index(2)]
        public string? VetSubSpecies { get; set; }
    }
    public class DrugInteraction
    {
        [Key]
        [Ignore] // Not imported from CSV.
        public int Id { get; set; }

        [Column("HASH")]
        [Ignore] // Generated in code.
        public string? Hash { get; set; }

        [Column("DDInterID_A")]
        [Name("DDInterID_A")]
        public string? DrugAID { get; set; }

        [Column("DDInterID_B")]
        [Name("DDInterID_B")]
        public string? DrugBID { get; set; }

        [Column("Drug_A")]
        [Name("Drug_A")]
        public string? DrugA { get; set; }

        [Column("Drug_B")]
        [Name("Drug_B")]
        public string? DrugB { get; set; }

        [Column("Level")]
        [Name("Level")]
        public string? Level { get; set; }

        [Ignore] // Not imported from CSV.
        public string? Description { get; set; }

        [Ignore] // Not imported from CSV.
        public string? Management { get; set; }

        [Ignore] // Not imported from CSV.
        public int InteractionID { get; set; }
    }
}
