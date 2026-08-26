using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a numeric precision between its core and serialized representations.
/// </summary>
public class NumericPrecisionMapper
    : IImmutableMapper<Dto.NumericPrecision?, Option<INumericPrecision>>
    , IImmutableMapper<Option<INumericPrecision>, Dto.NumericPrecision?>
{
    /// <summary>
    /// Maps an optional serialized numeric precision to its core representation.
    /// </summary>
    /// <param name="source">A serialized numeric precision, or <see langword="null"/> when the type has none.</param>
    /// <returns>The numeric precision, if the type has one.</returns>
    public Option<INumericPrecision> Map(Dto.NumericPrecision? source)
    {
        return source == null
            ? Option<INumericPrecision>.None
            : Option<INumericPrecision>.Some(new NumericPrecision(source.Precision, source.Scale));
    }

    /// <summary>
    /// Maps an optional numeric precision to its serialized representation.
    /// </summary>
    /// <param name="source">A numeric precision, if the type has one.</param>
    /// <returns>A serialized numeric precision, or <see langword="null"/> when the type has none.</returns>
    public Dto.NumericPrecision? Map(Option<INumericPrecision> source)
    {
        return source.MatchUnsafe(
            static p => new Dto.NumericPrecision { Precision = p.Precision, Scale = p.Scale },
            static () => (Dto.NumericPrecision?)null
        );
    }
}