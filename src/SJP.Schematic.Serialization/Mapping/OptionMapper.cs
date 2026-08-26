using Boxed.Mapping;
using LanguageExt;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps the optional values of a serialized definition, which are absent when <see langword="null"/>, to and from <see cref="Option{A}"/>.
/// </summary>
public class OptionMapper
    : IImmutableMapper<string?, Option<string>>
    , IImmutableMapper<Option<string>, string?>
    , IImmutableMapper<decimal?, Option<decimal>>
    , IImmutableMapper<Option<decimal>, decimal?>
{
    /// <summary>
    /// Maps an optional serialized string to its core representation.
    /// </summary>
    /// <param name="source">A string, or <see langword="null"/> when no value is available.</param>
    /// <returns>The string, if a value is available.</returns>
    public Option<string> Map(string? source)
    {
        return source == null ? Option<string>.None : Option<string>.Some(source);
    }

    /// <summary>
    /// Maps an optional string to its serialized representation.
    /// </summary>
    /// <param name="source">A string, if a value is available.</param>
    /// <returns>The string, or <see langword="null"/> when no value is available.</returns>
    public string? Map(Option<string> source)
    {
        return source.MatchUnsafe(static v => v, (string?)null);
    }

    /// <summary>
    /// Maps an optional serialized number to its core representation.
    /// </summary>
    /// <param name="source">A number, or <see langword="null"/> when no value is available.</param>
    /// <returns>The number, if a value is available.</returns>
    public Option<decimal> Map(decimal? source)
    {
        return !source.HasValue ? Option<decimal>.None : Option<decimal>.Some(source.Value);
    }

    /// <summary>
    /// Maps an optional number to its serialized representation.
    /// </summary>
    /// <param name="source">A number, if a value is available.</param>
    /// <returns>The number, or <see langword="null"/> when no value is available.</returns>
    public decimal? Map(Option<decimal> source)
    {
        return source.MatchUnsafe(static v => v, (decimal?)null);
    }
}