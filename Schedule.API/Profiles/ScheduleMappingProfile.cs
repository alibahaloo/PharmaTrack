using AutoMapper;
using PharmaTrack.Core.DTOs;
using Schedule.API.Data;

namespace Schedule.API.Profiles
{
    public class ScheduleMappingProfile : Profile
    {
        public ScheduleMappingProfile()
        {
            // Entity → DTO
            CreateMap<ScheduleTask, ScheduleTaskDto>();

            // DTO → Entity (for POST/PUT)
            CreateMap<ScheduleTaskDto, ScheduleTask>();
        }
    }
}
