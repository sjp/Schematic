using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseTriggerProfile : Profile
    {
        public DatabaseTriggerProfile()
        {
            CreateMap<Dto.DatabaseTrigger, DatabaseTrigger>();
            CreateMap<IDatabaseTrigger, Dto.DatabaseTrigger>();
        }
    }
}
