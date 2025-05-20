using System;
using System.Collections.Generic;

namespace PharmaTrack.DTOs.Drug
{
    public class DrugInfoDto
    {
        public DrugProductDto? Product { get; set; }
        public List<DrugIngredientDto> Ingredients { get; set; } = new List<DrugIngredientDto>();
        public List<DrugCompanyDto> Companies { get; set; } = new List<DrugCompanyDto>();
        public List<DrugStatusDto> Statuses { get; set; } = new List<DrugStatusDto>();
        public List<DrugFormDto> Forms { get; set; } = new List<DrugFormDto>();
        public List<DrugPackagingDto> Packagings { get; set; } = new List<DrugPackagingDto>();
        public List<DrugPharmaceuticalStdDto> PharmaceuticalStds { get; set; } = new List<DrugPharmaceuticalStdDto>();
        public List<DrugRouteDto> Routes { get; set; } = new List<DrugRouteDto>();
        public List<DrugScheduleDto> Schedules { get; set; } = new List<DrugScheduleDto>();
        public List<DrugTherapeuticClassDto> TherapeuticClasses { get; set; } = new List<DrugTherapeuticClassDto>();
        public List<DrugVeterinarySpeciesDto> VeterinarySpecies { get; set; } = new List<DrugVeterinarySpeciesDto>();
    }

    public class DrugProductDto
    {
        public int Id { get; set; }
        public string? Hash { get; set; }
        public int DrugCode { get; set; }
        public string? ProductCategorization { get; set; }
        public string? Class { get; set; }
        public string? DrugIdentificationNumber { get; set; }
        public string? BrandName { get; set; }
        public string? Descriptor { get; set; }
        public string? PediatricFlag { get; set; }
        public string? AccessionNumber { get; set; }
        public string? NumberOfAis { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? AiGroupNo { get; set; }
    }
    public class DrugIngredientDto
    {
        public int Id { get; set; }
        public string? Hash { get; set; }
        public int DrugCode { get; set; }
        public int ActiveIngredientCode { get; set; }
        public string? Ingredient { get; set; }
        public string? IngredientSuppliedInd { get; set; }
        public string? Strength { get; set; }
        public string? StrengthUnit { get; set; }
        public string? StrengthType { get; set; }
        public string? DosageValue { get; set; }
        public string? Base { get; set; }
        public string? DosageUnit { get; set; }
        public string? Notes { get; set; }
    }
    public class DrugCompanyDto
    {
        public int Id { get; set; }
        public string? Hash { get; set; }
        public int DrugCode { get; set; }
        public string? MfrCode { get; set; }
        public int CompanyCode { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyType { get; set; }
        public string? AddressMailingFlag { get; set; }
        public string? AddressBillingFlag { get; set; }
        public string? AddressNotificationFlag { get; set; }
        public string? AddressOther { get; set; }
        public string? SuiteNumber { get; set; }
        public string? StreetName { get; set; }
        public string? CityName { get; set; }
        public string? Province { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? PostOfficeBox { get; set; }
    }
    public class DrugStatusDto
    {
        public int Id { get; set; }
        public string? Hash { get; set; }
        public int DrugCode { get; set; }
        public string? CurrentStatusFlag { get; set; }
        public string? Status { get; set; }
        public DateTime? HistoryDate { get; set; }
    }
    public class DrugFormDto
    {
        public int Id { get; set; }
        public string? Hash { get; set; }
        public int DrugCode { get; set; }
        public int PharmFormCode { get; set; }
        public string? PharmaceuticalForm { get; set; }
    }
    public class DrugPackagingDto
    {
        public int Id { get; set; }
        public string? Hash { get; set; }
        public int DrugCode { get; set; }
        public string? Upc { get; set; }
        public string? PackageSizeUnit { get; set; }
        public string? PackageType { get; set; }
        public string? PackageSize { get; set; }
        public string? ProductInformation { get; set; }
    }
    public class DrugPharmaceuticalStdDto
    {
        public int Id { get; set; }
        public string? Hash { get; set; }
        public int DrugCode { get; set; }
        public string? PharmaceuticalStd { get; set; }
    }
    public class DrugRouteDto
    {
        public int Id { get; set; }
        public string? Hash { get; set; }
        public int DrugCode { get; set; }
        public int RouteOfAdministrationCode { get; set; }
        public string? RouteOfAdministration { get; set; }
    }
    public class DrugScheduleDto
    {
        public int Id { get; set; }
        public string? Hash { get; set; }
        public int DrugCode { get; set; }
        public string? Schedule { get; set; }
    }
    public class DrugTherapeuticClassDto
    {
        public int Id { get; set; }
        public string? Hash { get; set; }
        public int DrugCode { get; set; }
        public string? TcAtcNumber { get; set; }
        public string? TcAtc { get; set; }
        public string? TcAhfsNumber { get; set; }
        public string? TcAhfs { get; set; }
    }
    public class DrugVeterinarySpeciesDto
    {
        public int Id { get; set; }
        public string? Hash { get; set; }
        public int DrugCode { get; set; }
        public string? VetSpecies { get; set; }
        public string? VetSubSpecies { get; set; }
    }
    public class DrugInteractionDto
    {
        public int Id { get; set; }
        public string? Hash { get; set; }
        public string? DrugAID { get; set; }
        public string? DrugBID { get; set; }
        public string? DrugA { get; set; }
        public string? DrugB { get; set; }
        public string? Level { get; set; }
        public string? Description { get; set; }
        public string? Management { get; set; }
        public int InteractionID { get; set; }
    }
}
