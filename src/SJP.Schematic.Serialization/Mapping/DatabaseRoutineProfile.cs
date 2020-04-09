using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseRoutineProfile : Profile
    {
        public DatabaseRoutineProfile()
        {
            CreateMap<Dto.DatabaseRoutine, DatabaseRoutine>();
            CreateMap<IDatabaseRoutine, Dto.DatabaseRoutine>();
        }
    }
}
