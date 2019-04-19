using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class NumericPrecisionMappingExtensions
    {
        public static Dto.NumericPrecision ToDto(this Option<INumericPrecision> precision)
        {
            return precision.MatchUnsafe(
                p => new Dto.NumericPrecision { Precision = p.Precision, Scale = p.Scale },
                () => null
            );
        }

        public static Option<INumericPrecision> FromDto(this Dto.NumericPrecision dto)
        {
            return dto == null
                ? Option<INumericPrecision>.None
                : Option<INumericPrecision>.Some(new NumericPrecision(dto.Precision, dto.Scale));
        }
    }
}
