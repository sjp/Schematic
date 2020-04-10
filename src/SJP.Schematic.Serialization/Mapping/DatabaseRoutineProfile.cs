using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseRoutineProfile : Profile
    {
        public DatabaseRoutineProfile()
        {
            CreateMap<Dto.DatabaseRoutine, DatabaseRoutine>()
                .ConstructUsing((dto, ctx) => new DatabaseRoutine(
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.RoutineName!),
                    dto.Definition!
                ));
            CreateMap<IDatabaseRoutine, Dto.DatabaseRoutine>()
                .ForMember(dest => dest.RoutineName, src => src.MapFrom(r => r.Name));
        }
    }
}
