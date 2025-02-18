using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaTrack.Shared.DBModels
{
    public class DrugProduct
    {
        [Key]
        [Ignore] // Not imported from CSV.
        public int Id { get; set; }

        [Column("HASH")]
        [Ignore] // Generated in code.
        public string? Hash { get; set; }

        [Column("DRUG_CODE")]
        [Index(0)]
        public int DrugCode { get; set; }

        [Column("PRODUCT_CATEGORIZATION")]
        [Index(1)]
        public string? ProductCategorization { get; set; }

        [Column("CLASS")]
        [Index(2)]
        public string? Class { get; set; }

        [Column("DRUG_IDENTIFICATION_NUMBER")]
        [Index(3)]
        public string? DrugIdentificationNumber { get; set; }

        [Column("BRAND_NAME")]
        [Index(4)]
        public string? BrandName { get; set; }

        [Column("DESCRIPTOR")]
        [Index(5)]
        public string? Descriptor { get; set; }

        [Column("PEDIATRIC_FLAG")]
        [Index(6)]
        public string? PediatricFlag { get; set; }

        [Column("ACCESSION_NUMBER")]
        [Index(7)]
        public string? AccessionNumber { get; set; }

        [Column("NUMBER_OF_AIS")]
        [Index(8)]
        public string? NumberOfAis { get; set; }

        [Column("LAST_UPDATE_DATE")]
        [Index(9)]
        public DateTime? LastUpdateDate { get; set; }

        [Column("AI_GROUP_NO")]
        [Index(10)]
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
        [Index(0)]
        [Required]
        public int DrugCode { get; set; }

        [Column("ACTIVE_INGREDIENT_CODE")]
        [Index(1)]
        public int ActiveIngredientCode { get; set; }

        [Column("INGREDIENT")]
        [Index(2)]
        public string? Ingredient { get; set; }

        [Column("INGREDIENT_SUPPLIED_IND")]
        [Index(3)]
        public string? IngredientSuppliedInd { get; set; }

        [Column("STRENGTH")]
        [Index(4)]
        public string? Strength { get; set; }

        [Column("STRENGTH_UNIT")]
        [Index(5)]
        public string? StrengthUnit { get; set; }

        [Column("STRENGTH_TYPE")]
        [Index(6)]
        public string? StrengthType { get; set; }

        [Column("DOSAGE_VALUE")]
        [Index(7)]
        public string? DosageValue { get; set; }

        [Column("BASE")]
        [Index(8)]
        public string? Base { get; set; }

        [Column("DOSAGE_UNIT")]
        [Index(9)]
        public string? DosageUnit { get; set; }

        [Column("NOTES")]
        [Index(10)]
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
        [Index(0)]
        public int DrugCode { get; set; }

        [Column("MFR_CODE")]
        [Index(1)]
        public string? MfrCode { get; set; }

        [Column("COMPANY_CODE")]
        [Index(2)]
        public int CompanyCode { get; set; }

        [Column("COMPANY_NAME")]
        [Index(3)]
        public string? CompanyName { get; set; }

        [Column("COMPANY_TYPE")]
        [Index(4)]
        public string? CompanyType { get; set; }

        [Column("ADDRESS_MAILING_FLAG")]
        [Index(5)]
        public string? AddressMailingFlag { get; set; }

        [Column("ADDRESS_BILLING_FLAG")]
        [Index(6)]
        public string? AddressBillingFlag { get; set; }

        [Column("ADDRESS_NOTIFICATION_FLAG")]
        [Index(7)]
        public string? AddressNotificationFlag { get; set; }

        [Column("ADDRESS_OTHER")]
        [Index(8)]
        public string? AddressOther { get; set; }

        [Column("SUITE_NUMBER")]
        [Index(9)]
        public string? SuiteNumber { get; set; }

        [Column("STREET_NAME")]
        [Index(10)]
        public string? StreetName { get; set; }

        [Column("CITY_NAME")]
        [Index(11)]
        public string? CityName { get; set; }

        [Column("PROVINCE")]
        [Index(12)]
        public string? Province { get; set; }

        [Column("COUNTRY")]
        [Index(13)]
        public string? Country { get; set; }

        [Column("POSTAL_CODE")]
        [Index(14)]
        public string? PostalCode { get; set; }

        [Column("POST_OFFICE_BOX")]
        [Index(15)]
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
        [Index(0)]
        public int DrugCode { get; set; }

        [Column("CURRENT_STATUS_FLAG")]
        [Index(1)]
        public string? CurrentStatusFlag { get; set; }

        [Column("STATUS")]
        [Index(2)]
        public string? Status { get; set; }

        [Column("HISTORY_DATE")]
        [Index(3)]
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
        [Index(0)]
        public int DrugCode { get; set; }

        [Column("PHARM_FORM_CODE")]
        [Index(1)]
        public int PharmFormCode { get; set; }

        [Column("PHARMACEUTICAL_FORM")]
        [Index(2)]
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
        [Index(0)]
        public int DrugCode { get; set; }

        [Column("UPC")]
        [Index(1)]
        public string? Upc { get; set; }

        [Column("PACKAGE_SIZE_UNIT")]
        [Index(2)]
        public string? PackageSizeUnit { get; set; }

        [Column("PACKAGE_TYPE")]
        [Index(3)]
        public string? PackageType { get; set; }

        [Column("PACKAGE_SIZE")]
        [Index(4)]
        public string? PackageSize { get; set; }

        [Column("PRODUCT_INFORMATION")]
        [Index(5)]
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
        [Index(0)]
        public int DrugCode { get; set; }

        [Column("PHARMACEUTICAL_STD")]
        [Index(1)]
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
        [Index(0)]
        public int DrugCode { get; set; }

        [Column("ROUTE_OF_ADMINISTRATION_CODE")]
        [Required]
        [Index(1)]
        public int RouteOfAdministrationCode { get; set; }

        [Column("ROUTE_OF_ADMINISTRATION")]
        [Index(2)]
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
        [Index(0)]
        public int DrugCode { get; set; }

        [Column("SCHEDULE")]
        [Index(1)]
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
        [Index(0)]
        public int DrugCode { get; set; }

        [Column("TC_ATC_NUMBER")]
        [Index(1)]
        public string? TcAtcNumber { get; set; }

        [Column("TC_ATC")]
        [Index(2)]
        public string? TcAtc { get; set; }

        [Column("TC_AHFS_NUMBER")]
        [Index(3)]
        public string? TcAhfsNumber { get; set; }

        [Column("TC_AHFS")]
        [Index(4)]
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
        [Index(0)]
        public int DrugCode { get; set; }

        [Column("VET_SPECIES")]
        [Index(1)]
        public string? VetSpecies { get; set; }

        [Column("VET_SUB_SPECIES")]
        [Index(2)]
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
        [Index(0)]
        public string? DrugAID { get; set; }

        [Column("DDInterID_B")]
        [Index(1)]
        public string? DrugBID { get; set; }

        [Column("Drug_A")]
        [Index(2)]
        public string? DrugA { get; set; }

        [Column("Drug_B")]
        [Index(3)]
        public string? DrugB { get; set; }

        [Column("Level")]
        [Index(4)]
        public string? Level { get; set; }

        [Ignore] // Not imported from CSV.
        public string? Description { get; set; }

        [Ignore] // Not imported from CSV.
        public string? Management { get; set; }

        [Ignore] // Not imported from CSV.
        public int InteractionID { get; set; }
    }

}