using AutoMapper;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseCheckProfile : Profile
    {
        public DatabaseCheckProfile()
        {
            CreateMap<Dto.DatabaseCheckConstraint, DatabaseCheckConstraint>()
                .ConstructUsing(static (dto, ctx) => new DatabaseCheckConstraint(
                    ctx.Mapper.Map<Dto.Identifier?, Option<Identifier>>(dto.CheckName),
                    dto.Definition!,
                    dto.IsEnabled
                ))
                .ForAllMembers(static cfg => cfg.Ignore());
            CreateMap<IDatabaseCheckConstraint, Dto.DatabaseCheckConstraint>()
                .ForMember(static dest => dest.CheckName, static src => src.MapFrom(static ck => ck.Name));
        }
    }
}
