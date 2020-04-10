using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseSynonymProfile : Profile
    {
        public DatabaseSynonymProfile()
        {
            CreateMap<Dto.DatabaseSynonym, DatabaseSynonym>()
                .ConstructUsing((dto, ctx) => new DatabaseSynonym(
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.SynonymName!),
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.Target!)
                ));
            CreateMap<IDatabaseSynonym, Dto.DatabaseSynonym>()
                .ForMember(dest => dest.SynonymName, src => src.MapFrom(s => s.Name));
        }
    }
}
