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
                .ConstructUsing(static (dto, ctx) => new DatabaseSequence(
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.SequenceName!),
                    dto.Start,
                    dto.Increment,
                    ctx.Mapper.Map<decimal?, Option<decimal>>(dto.MinValue),
                    ctx.Mapper.Map<decimal?, Option<decimal>>(dto.MaxValue),
                    dto.Cycle,
                    dto.Cache
                ))
                .ForAllMembers(static cfg => cfg.Ignore());
            CreateMap<IDatabaseSequence, Dto.DatabaseSequence>()
                .ForMember(static dest => dest.SequenceName, static src => src.MapFrom(static s => s.Name));
        }
    }
}
