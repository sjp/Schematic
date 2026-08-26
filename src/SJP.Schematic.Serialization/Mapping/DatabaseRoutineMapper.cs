using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a database routine between its core and serialized representations.
/// </summary>
public class DatabaseRoutineMapper
    : IImmutableMapper<Dto.DatabaseRoutine, IDatabaseRoutine>
    , IImmutableMapper<IDatabaseRoutine, Dto.DatabaseRoutine>
{
    /// <summary>
    /// Maps a serialized routine to its core representation.
    /// </summary>
    /// <param name="source">A serialized routine.</param>
    /// <returns>A routine.</returns>
    public IDatabaseRoutine Map(Dto.DatabaseRoutine source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();

        return new DatabaseRoutine(
            identifierMapper.Map(source.RoutineName),
            source.Definition
        );
    }

    /// <summary>
    /// Maps a routine to its serialized representation.
    /// </summary>
    /// <param name="source">A routine.</param>
    /// <returns>A serialized routine.</returns>
    public Dto.DatabaseRoutine Map(IDatabaseRoutine source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();

        return new Dto.DatabaseRoutine
        {
            RoutineName = identifierMapper.Map(source.Name),
            Definition = source.Definition,
        };
    }
}