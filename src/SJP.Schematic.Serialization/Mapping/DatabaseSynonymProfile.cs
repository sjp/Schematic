using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseSynonymProfile : Profile
    {
        public DatabaseSynonymProfile()
        {
            CreateMap<Dto.DatabaseSynonym, DatabaseSynonym>()
                .ConstructUsing(static (dto, ctx) => new DatabaseSynonym(
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.SynonymName!),
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.Target!)
                ))
                .ForAllMembers(static cfg => cfg.Ignore());
            CreateMap<IDatabaseSynonym, Dto.DatabaseSynonym>()
                .ForMember(static dest => dest.SynonymName, static src => src.MapFrom(static s => s.Name));
        }
    }
}
