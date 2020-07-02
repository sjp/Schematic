using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseTriggerProfile : Profile
    {
        public DatabaseTriggerProfile()
        {
            CreateMap<Dto.DatabaseTrigger, DatabaseTrigger>()
                .ConstructUsing((dto, ctx) => new DatabaseTrigger(
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.TriggerName!),
                    dto.Definition!,
                    dto.QueryTiming,
                    dto.TriggerEvent,
                    dto.IsEnabled
                ))
                .ForAllMembers(cfg => cfg.Ignore());
            CreateMap<IDatabaseTrigger, Dto.DatabaseTrigger>()
                .ForMember(dest => dest.TriggerName, src => src.MapFrom(tr => tr.Name));
        }
    }
}
