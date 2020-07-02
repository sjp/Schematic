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
                .ConstructUsing((dto, ctx) => new DatabaseCheckConstraint(
                    ctx.Mapper.Map<Dto.Identifier?, Option<Identifier>>(dto.CheckName),
                    dto.Definition!,
                    dto.IsEnabled
                ))
                .ForAllMembers(cfg => cfg.Ignore());
            CreateMap<IDatabaseCheckConstraint, Dto.DatabaseCheckConstraint>()
                .ForMember(dest => dest.CheckName, src => src.MapFrom(ck => ck.Name));
        }
    }
}
