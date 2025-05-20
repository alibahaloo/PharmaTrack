using AutoMapper;
using Drug.API.Data;
using PharmaTrack.DTOs.Drug;

namespace Drug.API.Profiles
{
    public class DrugMappingProfile : Profile
    {
        public DrugMappingProfile()
        {
            CreateMap<DrugProduct, DrugProductDto>();
            CreateMap<DrugIngredient, DrugIngredientDto>();
            CreateMap<DrugCompany, DrugCompanyDto>();
            CreateMap<DrugStatus, DrugStatusDto>();
            CreateMap<DrugForm, DrugFormDto>();
            CreateMap<DrugPackaging, DrugPackagingDto>();
            CreateMap<DrugPharmaceuticalStd, DrugPharmaceuticalStdDto>();
            CreateMap<DrugRoute, DrugRouteDto>();
            CreateMap<DrugSchedule, DrugScheduleDto>();
            CreateMap<DrugTherapeuticClass, DrugTherapeuticClassDto>();
            CreateMap<DrugVeterinarySpecies, DrugVeterinarySpeciesDto>();
            CreateMap<DrugInteraction, DrugInteractionDto>();
        }
    }
}
