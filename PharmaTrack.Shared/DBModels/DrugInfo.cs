using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaTrack.Shared.DBModels
{
    public class DrugProduct
    {
        [Key]
        public int Id { get; set; }
        [Column("HASH")]
        public string? Hash { get; set; }

        [Column("DRUG_CODE")]
        public int DrugCode { get; set; }

        [Column("PRODUCT_CATEGORIZATION")]
        public string? ProductCategorization { get; set; }

        [Column("CLASS")]
        public string? Class { get; set; }

        [Column("DIN")]
        public string? DrugIdentificationNumber { get; set; }

        [Column("BRAND_NAME")]
        public string? BrandName { get; set; } = null!;

        [Column("DESCRIPTOR")]
        public string? Descriptor { get; set; }

        [Column("PEDIATRIC_FLAG")]
        public string? PediatricFlag { get; set; }

        [Column("ACCESSION_NUMBER")]
        public string? AccessionNumber { get; set; }

        [Column("NUMBER_OF_AIS")]
        public string? NumberOfAis { get; set; }

        [Column("LAST_UPDATE_DATE")]
        public DateTime? LastUpdateDate { get; set; }

        [Column("AI_GROUP_NO")]
        public string? AiGroupNo { get; set; }
    }

    public class DrugIngredient
    {
        [Key]
        public int Id { get; set; }
        [Column("HASH")]
        public string? Hash { get; set; }
        [Column("ACTIVE_INGREDIENT_CODE")]
        public int ActiveIngredientCode { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("INGREDIENT")]
        public string? Ingredient { get; set; }

        [Column("INGREDIENT_SUPPLIED_IND")]
        public string? IngredientSuppliedInd { get; set; }

        [Column("STRENGTH")]
        public string? Strength { get; set; }

        [Column("STRENGTH_UNIT")]
        public string? StrengthUnit { get; set; }

        [Column("STRENGTH_TYPE")]
        public string? StrengthType { get; set; }

        [Column("DOSAGE_VALUE")]
        public string? DosageValue { get; set; }

        [Column("BASE")]
        public string? Base { get; set; }

        [Column("DOSAGE_UNIT")]
        public string? DosageUnit { get; set; }

        [Column("NOTES")]
        public string? Notes { get; set; }
    }

    public class DrugCompany
    {
        [Key]
        public int Id { get; set; }
        [Column("HASH")]
        public string? Hash { get; set; }
        [Column("COMPANY_CODE")]
        public int CompanyCode { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("MFR_CODE")]
        public string? MfrCode { get; set; }

        [Column("COMPANY_NAME")]
        public string? CompanyName { get; set; }

        [Column("COMPANY_TYPE")]
        public string? CompanyType { get; set; }

        [Column("ADDRESS_MAILING_FLAG")]
        public string? AddressMailingFlag { get; set; }

        [Column("ADDRESS_BILLING_FLAG")]
        public string? AddressBillingFlag { get; set; }

        [Column("ADDRESS_NOTIFICATION_FLAG")]
        public string? AddressNotificationFlag { get; set; }

        [Column("ADDRESS_OTHER")]
        public string? AddressOther { get; set; }

        [Column("SUITE_NUMBER")]
        public string? SuiteNumber { get; set; }

        [Column("STREET_NAME")]
        public string? StreetName { get; set; }

        [Column("CITY_NAME")]
        public string? CityName { get; set; }

        [Column("PROVINCE")]
        public string? Province { get; set; }

        [Column("COUNTRY")]
        public string? Country { get; set; }

        [Column("POSTAL_CODE")]
        public string? PostalCode { get; set; }

        [Column("POST_OFFICE_BOX")]
        public string? PostOfficeBox { get; set; }
    }

    public class DrugStatus
    {
        [Key]
        public int Id { get; set; }
        [Column("HASH")]
        public string? Hash { get; set; }
        [Column("STATUS_ID")]
        public int StatusId { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("STATUS")]
        public string? Status { get; set; }

        [Column("HISTORY_DATE")]
        public DateTime? HistoryDate { get; set; }
    }

    public class DrugForm
    {
        [Key]
        public int Id { get; set; }
        [Column("HASH")]
        public string? Hash { get; set; }
        [Column("PHARM_FORM_CODE")]
        public int PharmFormCode { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("PHARMACEUTICAL_FORM")]
        public string? PharmaceuticalForm { get; set; }
    }

    public class DrugPackaging
    {
        [Key]
        public int Id { get; set; }
        [Column("HASH")]
        public string? Hash { get; set; }
        [Column("PACKAGE_ID")]
        public int PackageId { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("UPC")]
        public string? Upc { get; set; }

        [Column("PACKAGE_SIZE_UNIT")]
        public string? PackageSizeUnit { get; set; }

        [Column("PACKAGE_TYPE")]
        public string? PackageType { get; set; }

        [Column("PACKAGE_SIZE")]
        public string? PackageSize { get; set; }

        [Column("PRODUCT_INFORMATION")]
        public string? ProductInformation { get; set; }
    }

    public class DrugPharmaceuticalStd
    {
        [Key]
        public int Id { get; set; }
        [Column("HASH")]
        public string? Hash { get; set; }
        [Column("PHARM_STD_ID")]
        public int PharmStdId { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("PHARMACEUTICAL_STD")]
        public string? PharmaceuticalStd { get; set; }
    }

    public class DrugRoute
    {
        [Key]
        public int Id { get; set; }
        [Column("HASH")]
        public string? Hash { get; set; }
        [Column("ROUTE_ID")]
        public int RouteId { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("ROUTE_OF_ADMINISTRATION_CODE")]
        [Required]
        public int RouteOfAdministrationCode { get; set; }

        [Column("ROUTE_OF_ADMINISTRATION")]
        public string? RouteOfAdministration { get; set; }
    }

    public class DrugSchedule
    {
        [Key]
        public int Id { get; set; }
        [Column("HASH")]
        public string? Hash { get; set; }
        [Column("SCHEDULE_ID")]
        public int ScheduleId { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("SCHEDULE")]
        public string? Schedule { get; set; }
    }

    public class DrugTherapeuticClass
    {
        [Key]
        public int Id { get; set; }
        [Column("HASH")]
        public string? Hash { get; set; }
        [Column("THER_CLASS_ID")]
        public int TherClassId { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("TC_ATC_NUMBER")]
        public string? TcAtcNumber { get; set; }

        [Column("TC_ATC")]
        public string? TcAtc { get; set; }

        [Column("TC_AHFS_NUMBER")]
        public string? TcAhfsNumber { get; set; }

        [Column("TC_AHFS")]
        public string? TcAhfs { get; set; }
    }

    public class DrugVeterinarySpecies
    {
        [Key]
        public int Id { get; set; }
        [Column("HASH")]
        public string? Hash { get; set; }
        [Column("VET_SPECIES_ID")]
        public int VetSpeciesId { get; set; }

        [Column("DRUG_CODE")]
        [Required]
        public int DrugCode { get; set; }

        [Column("VET_SPECIES")]
        public string? VetSpecies { get; set; }

        [Column("VET_SUB_SPECIES")]
        public string? VetSubSpecies { get; set; }
    }
}