using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class DatabaseSynonymMapper
    : IImmutableMapper<Dto.DatabaseSynonym, IDatabaseSynonym>
    , IImmutableMapper<IDatabaseSynonym, Dto.DatabaseSynonym>
{
    public Dto.DatabaseSynonym Map(IDatabaseSynonym source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();

        return new Dto.DatabaseSynonym
        {
            SynonymName = identifierMapper.Map(source.Name),
            Target = identifierMapper.Map(source.Target)
        };
    }

    public IDatabaseSynonym Map(Dto.DatabaseSynonym source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier?, Identifier>();

        return new DatabaseSynonym(
            identifierMapper.Map(source.SynonymName),
            identifierMapper.Map(source.Target)
        );
    }
}
