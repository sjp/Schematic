using System.Collections.Generic;
using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class RelationalDatabaseProfile : Profile
{
    public RelationalDatabaseProfile()
    {
        CreateMap<Dto.RelationalDatabase, RelationalDatabase>()
            .ConstructUsing(static (dto, ctx) => new RelationalDatabase(
                ctx.Mapper.Map<Dto.IdentifierDefaults, IdentifierDefaults>(dto.IdentifierDefaults),
                dto.IdentifierResolver ?? new VerbatimIdentifierResolutionStrategy(),
                ctx.Mapper.Map<IEnumerable<Dto.RelationalDatabaseTable>, IEnumerable<RelationalDatabaseTable>>(dto.Tables),
                ctx.Mapper.Map<IEnumerable<Dto.DatabaseView>, IEnumerable<DatabaseView>>(dto.Views),
                ctx.Mapper.Map<IEnumerable<Dto.DatabaseSequence>, IEnumerable<DatabaseSequence>>(dto.Sequences),
                ctx.Mapper.Map<IEnumerable<Dto.DatabaseSynonym>, IEnumerable<DatabaseSynonym>>(dto.Synonyms),
                ctx.Mapper.Map<IEnumerable<Dto.DatabaseRoutine>, IEnumerable<DatabaseRoutine>>(dto.Routines)
            ))
            .ForAllMembers(static cfg => cfg.Ignore());
    }
}
