using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a database synonym between its core and serialized representations.
/// </summary>
public class DatabaseSynonymMapper
    : IImmutableMapper<Dto.DatabaseSynonym, IDatabaseSynonym>
    , IImmutableMapper<IDatabaseSynonym, Dto.DatabaseSynonym>
{
    /// <summary>
    /// Maps a synonym to its serialized representation.
    /// </summary>
    /// <param name="source">A synonym.</param>
    /// <returns>A serialized synonym.</returns>
    public Dto.DatabaseSynonym Map(IDatabaseSynonym source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();

        return new Dto.DatabaseSynonym
        {
            SynonymName = identifierMapper.Map(source.Name),
            Target = identifierMapper.Map(source.Target),
        };
    }

    /// <summary>
    /// Maps a serialized synonym to its core representation.
    /// </summary>
    /// <param name="source">A serialized synonym.</param>
    /// <returns>A synonym.</returns>
    public IDatabaseSynonym Map(Dto.DatabaseSynonym source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();

        return new DatabaseSynonym(
            identifierMapper.Map(source.SynonymName),
            identifierMapper.Map(source.Target)
        );
    }
}