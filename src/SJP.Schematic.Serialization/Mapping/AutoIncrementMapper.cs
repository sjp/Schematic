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
        return source == null
            ? Option<IAutoIncrement>.None
            : Option<IAutoIncrement>.Some(new AutoIncrement(source.InitialValue, source.Increment));
    }

    /// <summary>
    /// Maps an auto-incrementing sequence to its serialized representation.
    /// </summary>
    /// <param name="source">An auto-incrementing sequence, if the column has one.</param>
    /// <returns>A serialized auto-incrementing sequence, or <see langword="null"/> when the column has none.</returns>
    public Dto.AutoIncrement? Map(Option<IAutoIncrement> source)
    {
        return source.MatchUnsafe(
            static incr => new Dto.AutoIncrement
            {
                Increment = incr.Increment,
                InitialValue = incr.InitialValue,
            },
            static () => (Dto.AutoIncrement?)null
        );
    }
}