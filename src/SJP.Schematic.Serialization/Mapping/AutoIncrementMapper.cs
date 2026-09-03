using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps an auto-incrementing sequence between its core and serialized representations.
/// </summary>
public class AutoIncrementMapper
    : IImmutableMapper<Dto.AutoIncrement?, Option<IAutoIncrement>>
    , IImmutableMapper<Option<IAutoIncrement>, Dto.AutoIncrement?>
{
    /// <summary>
    /// Maps a serialized auto-incrementing sequence to its core representation.
    /// </summary>
    /// <param name="source">A serialized auto-incrementing sequence, or <see langword="null"/> when the column has none.</param>
    /// <returns>The auto-incrementing sequence, if the column has one.</returns>
    public Option<IAutoIncrement> Map(Dto.AutoIncrement? source)
    {
        if (source == null)
            return Option<IAutoIncrement>.None;

        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier?, Option<Identifier>>();
        var decimalMapper = MapperRegistry.GetMapper<decimal?, Option<decimal>>();

        return Option<IAutoIncrement>.Some(new AutoIncrement(
            source.InitialValue,
            source.Increment,
            source.Generation,
            decimalMapper.Map(source.MinValue),
            decimalMapper.Map(source.MaxValue),
            source.Cycle,
            identifierMapper.Map(source.SequenceName)
        ));
    }

    /// <summary>
    /// Maps an auto-incrementing sequence to its serialized representation.
    /// </summary>
    /// <param name="source">An auto-incrementing sequence, if the column has one.</param>
    /// <returns>A serialized auto-incrementing sequence, or <see langword="null"/> when the column has none.</returns>
    public Dto.AutoIncrement? Map(Option<IAutoIncrement> source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Option<Identifier>, Dto.Identifier?>();
        var decimalMapper = MapperRegistry.GetMapper<Option<decimal>, decimal?>();

        return source.MatchUnsafe(
            incr => new Dto.AutoIncrement
            {
                Increment = incr.Increment,
                InitialValue = incr.InitialValue,
                Generation = incr.Generation,
                MinValue = decimalMapper.Map(incr.MinValue),
                MaxValue = decimalMapper.Map(incr.MaxValue),
                Cycle = incr.Cycle,
                SequenceName = identifierMapper.Map(incr.SequenceName),
            },
            static () => (Dto.AutoIncrement?)null
        );
    }
}