using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class AutoIncrementMappingExtensions
    {
        public static Dto.AutoIncrement ToDto(this Option<IAutoIncrement> autoIncrement)
        {
            return autoIncrement.MatchUnsafe(
                incr => new Dto.AutoIncrement
                {
                    Increment = incr.Increment,
                    InitialValue = incr.InitialValue
                },
                () => null
            );
        }

        public static Option<IAutoIncrement> FromDto(this Dto.AutoIncrement dto)
        {
            return dto == null
                ? Option<IAutoIncrement>.None
                : Option<IAutoIncrement>.Some(new AutoIncrement(dto.InitialValue, dto.Increment));
        }
    }
}
