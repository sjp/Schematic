using Boxed.Mapping;
using LanguageExt;

namespace SJP.Schematic.Serialization.Mapping;

public class OptionMapper
    : IImmutableMapper<string?, Option<string>>
    , IImmutableMapper<Option<string>, string?>
    , IImmutableMapper<decimal?, Option<decimal>>
    , IImmutableMapper<Option<decimal>, decimal?>
{
    public Option<string> Map(string? source)
    {
        return source == null ? Option<string>.None : Option<string>.Some(source);
    }

    public string? Map(Option<string> source)
    {
        return source.MatchUnsafe(static v => v, (string?)null);
    }

    public Option<decimal> Map(decimal? source)
    {
        return !source.HasValue ? Option<decimal>.None : Option<decimal>.Some(source.Value);
    }

    public decimal? Map(Option<decimal> source)
    {
        return source.MatchUnsafe(static v => v, (decimal?)null);
    }
}