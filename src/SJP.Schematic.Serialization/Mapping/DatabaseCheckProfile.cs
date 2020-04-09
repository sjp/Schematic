using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseCheckProfile : Profile
    {
        public DatabaseCheckProfile()
        {
            CreateMap<Dto.DatabaseCheckConstraint, DatabaseCheckConstraint>();
            CreateMap<IDatabaseCheckConstraint, Dto.DatabaseCheckConstraint>();
        }
    }
}
