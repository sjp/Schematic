using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a database sequence between its core and serialized representations.
/// </summary>
public class DatabaseSequenceMapper
    : IImmutableMapper<Dto.DatabaseSequence, IDatabaseSequence>
    , IImmutableMapper<IDatabaseSequence, Dto.DatabaseSequence>
{
    /// <summary>
    /// Maps a serialized sequence to its core representation.
    /// </summary>
    /// <param name="source">A serialized sequence.</param>
    /// <returns>A sequence.</returns>
    public IDatabaseSequence Map(Dto.DatabaseSequence source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var decimalMapper = MapperRegistry.GetMapper<decimal?, Option<decimal>>();

        return new DatabaseSequence(
            identifierMapper.Map(source.SequenceName),
            source.Start,
            source.Increment,
            decimalMapper.Map(source.MinValue),
            decimalMapper.Map(source.MaxValue),
            source.Cycle,
            source.Cache
        );
    }

    /// <summary>
    /// Maps a sequence to its serialized representation.
    /// </summary>
    /// <param name="source">A sequence.</param>
    /// <returns>A serialized sequence.</returns>
    public Dto.DatabaseSequence Map(IDatabaseSequence source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var decimalMapper = MapperRegistry.GetMapper<Option<decimal>, decimal?>();

        return new Dto.DatabaseSequence
        {
            SequenceName = identifierMapper.Map(source.Name),
            Start = source.Start,
            Increment = source.Increment,
            MinValue = decimalMapper.Map(source.MinValue),
            MaxValue = decimalMapper.Map(source.MaxValue),
            Cycle = source.Cycle,
            Cache = source.Cache,
        };
    }
}