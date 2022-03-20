using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class NumericPrecisionProfile
    : IImmutableMapper<Dto.NumericPrecision?, Option<INumericPrecision>>
    , IImmutableMapper<Option<INumericPrecision>, Dto.NumericPrecision?>
{
    public Option<INumericPrecision> Map(Dto.NumericPrecision? source)
    {
        return source == null
            ? Option<INumericPrecision>.None
            : Option<INumericPrecision>.Some(new NumericPrecision(source.Precision, source.Scale));
    }

    public Dto.NumericPrecision? Map(Option<INumericPrecision> source)
    {
        return source.MatchUnsafe(
            static p => new Dto.NumericPrecision { Precision = p.Precision, Scale = p.Scale },
            static () => (Dto.NumericPrecision?)null
        );
    }
}
