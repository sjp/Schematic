using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseRoutineProfile : Profile
    {
        public DatabaseRoutineProfile()
        {
            CreateMap<Dto.DatabaseRoutine, DatabaseRoutine>()
                .ConstructUsing(static (dto, ctx) => new DatabaseRoutine(
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.RoutineName!),
                    dto.Definition!
                ))
                .ForAllMembers(static cfg => cfg.Ignore());
            CreateMap<IDatabaseRoutine, Dto.DatabaseRoutine>()
                .ForMember(static dest => dest.RoutineName, static src => src.MapFrom(static r => r.Name));
        }
    }
}
