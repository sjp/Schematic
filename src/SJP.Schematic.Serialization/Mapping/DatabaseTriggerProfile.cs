using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseTriggerProfile : Profile
    {
        public DatabaseTriggerProfile()
        {
            CreateMap<Dto.DatabaseTrigger, DatabaseTrigger>()
                .ConstructUsing(static (dto, ctx) => new DatabaseTrigger(
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.TriggerName!),
                    dto.Definition!,
                    dto.QueryTiming,
                    dto.TriggerEvent,
                    dto.IsEnabled
                ))
                .ForAllMembers(static cfg => cfg.Ignore());
            CreateMap<IDatabaseTrigger, Dto.DatabaseTrigger>()
                .ForMember(static dest => dest.TriggerName, static src => src.MapFrom(static tr => tr.Name));
        }
    }
}
