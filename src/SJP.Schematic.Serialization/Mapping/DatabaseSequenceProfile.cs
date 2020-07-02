using AutoMapper;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseSequenceProfile : Profile
    {
        public DatabaseSequenceProfile()
        {
            CreateMap<Dto.DatabaseSequence, DatabaseSequence>()
                .ConstructUsing((dto, ctx) => new DatabaseSequence(
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.SequenceName!),
                    dto.Start,
                    dto.Increment,
                    ctx.Mapper.Map<decimal?, Option<decimal>>(dto.MinValue),
                    ctx.Mapper.Map<decimal?, Option<decimal>>(dto.MaxValue),
                    dto.Cycle,
                    dto.Cache
                ))
                .ForAllMembers(cfg => cfg.Ignore());
            CreateMap<IDatabaseSequence, Dto.DatabaseSequence>()
                .ForMember(dest => dest.SequenceName, src => src.MapFrom(s => s.Name));
        }
    }
}
