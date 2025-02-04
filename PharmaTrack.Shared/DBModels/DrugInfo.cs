using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaTrack.Shared.DBModels
{
    public class DrugProduct
    {
        [Key]
        public int Id { get; set; }

        [Column("DRUG_CODE")]
        public int DrugCode { get; set; }

        [Column("PRODUCT_CATEGORIZATION")]
        [MaxLength(80)]
        public string? ProductCategorization { get; set; }

        [Column("CLASS")]
        [MaxLength(40)]
        public string? Class { get; set; }

        [Column("DIN")]
        [MaxLength(29)]
        public string? DrugIdentificationNumber { get; set; }

        [Column("BRAND_NAME")]
        [MaxLength(200)]
        public string? BrandName { get; set; } = null!;

        [Column("DESCRIPTOR")]
        [MaxLength(150)]
        public string? Descriptor { get; set; }

        [Column("PEDIATRIC_FLAG")]
        [MaxLength(1)]
        public string? PediatricFlag { get; set; }

        [Column("ACCESSION_NUMBER")]
        [MaxLength(5)]
        public string? AccessionNumber { get; set; }

        [Column("NUMBER_OF_AIS")]
        [MaxLength(10)]
        public string? NumberOfAis { get; set; }

        [Column("LAST_UPDATE_DATE")]
        public DateTime? LastUpdateDate { get; set; }

        [Column("AI_GROUP_NO")]
        [MaxLength(10)]
        public string? AiGroupNo { get; set; }
    }

    public class DrugIngredient
    {
        [Key]
        public int Id { get; set; }

        [Column("ACTIVE_INGREDIENT_CODE")]
        public int ActiveIngredientCode { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("INGREDIENT")]
        [MaxLength(240)]
        public string? Ingredient { get; set; }

        [Column("INGREDIENT_SUPPLIED_IND")]
        [MaxLength(1)]
        public string? IngredientSuppliedInd { get; set; }

        [Column("STRENGTH")]
        [MaxLength(20)]
        public string? Strength { get; set; }

        [Column("STRENGTH_UNIT")]
        [MaxLength(40)]
        public string? StrengthUnit { get; set; }

        [Column("STRENGTH_TYPE")]
        [MaxLength(40)]
        public string? StrengthType { get; set; }

        [Column("DOSAGE_VALUE")]
        [MaxLength(20)]
        public string? DosageValue { get; set; }

        [Column("BASE")]
        [MaxLength(1)]
        public string? Base { get; set; }

        [Column("DOSAGE_UNIT")]
        [MaxLength(40)]
        public string? DosageUnit { get; set; }

        [Column("NOTES")]
        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class DrugCompany
    {
        [Key]
        public int Id { get; set; }

        [Column("COMPANY_CODE")]
        public int CompanyCode { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("MFR_CODE")]
        [MaxLength(5)]
        public string? MfrCode { get; set; }

        [Column("COMPANY_NAME")]
        [Required]
        [MaxLength(80)]
        public string CompanyName { get; set; } = null!;

        [Column("COMPANY_TYPE")]
        [MaxLength(40)]
        public string? CompanyType { get; set; }

        [Column("ADDRESS_MAILING_FLAG")]
        [MaxLength(1)]
        public string? AddressMailingFlag { get; set; }

        [Column("ADDRESS_BILLING_FLAG")]
        [MaxLength(1)]
        public string? AddressBillingFlag { get; set; }

        [Column("ADDRESS_NOTIFICATION_FLAG")]
        [MaxLength(1)]
        public string? AddressNotificationFlag { get; set; }

        [Column("ADDRESS_OTHER")]
        [MaxLength(1)]
        public string? AddressOther { get; set; }

        [Column("SUITE_NUMBER")]
        [MaxLength(20)]
        public string? SuiteNumber { get; set; }

        [Column("STREET_NAME")]
        [MaxLength(80)]
        public string? StreetName { get; set; }

        [Column("CITY_NAME")]
        [MaxLength(60)]
        public string? CityName { get; set; }

        [Column("PROVINCE")]
        [MaxLength(40)]
        public string? Province { get; set; }

        [Column("COUNTRY")]
        [MaxLength(40)]
        public string? Country { get; set; }

        [Column("POSTAL_CODE")]
        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [Column("POST_OFFICE_BOX")]
        [MaxLength(15)]
        public string? PostOfficeBox { get; set; }
    }

    public class DrugStatus
    {
        [Key]
        public int Id { get; set; }

        [Column("STATUS_ID")]
        public int StatusId { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("STATUS")]
        [MaxLength(40)]
        public string? Status { get; set; }

        [Column("HISTORY_DATE")]
        public DateTime? HistoryDate { get; set; }
    }

    public class DrugForm
    {
        [Key]
        public int Id { get; set; }

        [Column("PHARM_FORM_CODE")]
        public int PharmFormCode { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("PHARMACEUTICAL_FORM")]
        [MaxLength(40)]
        public string? PharmaceuticalForm { get; set; }
    }

    public class DrugPackaging
    {
        [Key]
        public int Id { get; set; }

        [Column("PACKAGE_ID")]
        public int PackageId { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("UPC")]
        [MaxLength(12)]
        public string? Upc { get; set; }

        [Column("PACKAGE_SIZE_UNIT")]
        [MaxLength(40)]
        public string? PackageSizeUnit { get; set; }

        [Column("PACKAGE_TYPE")]
        [MaxLength(40)]
        public string? PackageType { get; set; }

        [Column("PACKAGE_SIZE")]
        [MaxLength(5)]
        public string? PackageSize { get; set; }

        [Column("PRODUCT_INFORMATION")]
        [MaxLength(80)]
        public string? ProductInformation { get; set; }
    }

    public class DrugPharmaceuticalStd
    {
        [Key]
        public int Id { get; set; }

        [Column("PHARM_STD_ID")]
        public int PharmStdId { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("PHARMACEUTICAL_STD")]
        [MaxLength(40)]
        public string? PharmaceuticalStd { get; set; }
    }

    public class DrugRoute
    {
        [Key]
        public int Id { get; set; }

        [Column("ROUTE_ID")]
        public int RouteId { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("ROUTE_OF_ADMINISTRATION_CODE")]
        [Required]
        public int RouteOfAdministrationCode { get; set; }

        [Column("ROUTE_OF_ADMINISTRATION")]
        [MaxLength(40)]
        public string? RouteOfAdministration { get; set; }
    }

    public class DrugSchedule
    {
        [Key]
        public int Id { get; set; }

        [Column("SCHEDULE_ID")]
        public int ScheduleId { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("SCHEDULE")]
        [MaxLength(40)]
        public string? Schedule { get; set; }
    }

    public class DrugTherapeuticClass
    {
        [Key]
        public int Id { get; set; }

        [Column("THER_CLASS_ID")]
        public int TherClassId { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("TC_ATC_NUMBER")]
        [MaxLength(8)]
        public string? TcAtcNumber { get; set; }

        [Column("TC_ATC")]
        [MaxLength(120)]
        public string? TcAtc { get; set; }

        [Column("TC_AHFS_NUMBER")]
        [MaxLength(20)]
        public string? TcAhfsNumber { get; set; }

        [Column("TC_AHFS")]
        [MaxLength(80)]
        public string? TcAhfs { get; set; }
    }

    public class DrugVeterinarySpecies
    {
        [Key]
        public int Id { get; set; }

        [Column("VET_SPECIES_ID")]
        public int VetSpeciesId { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("VET_SPECIES")]
        [MaxLength(80)]
        public string? VetSpecies { get; set; }

        [Column("VET_SUB_SPECIES")]
        [MaxLength(80)]
        public string? VetSubSpecies { get; set; }
    }
}