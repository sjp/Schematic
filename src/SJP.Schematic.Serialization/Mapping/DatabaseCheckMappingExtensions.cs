using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class DatabaseCheckMappingExtensions
    {
        public static Dto.DatabaseCheckConstraint ToDto(this IDatabaseCheckConstraint check)
        {
            if (check == null)
                throw new ArgumentNullException(nameof(check));

            var checkName = check.Name.MatchUnsafe(name => name.ToDto(), () => null);

            return new Dto.DatabaseCheckConstraint
            {
                Definition = check.Definition,
                IsEnabled = check.IsEnabled,
                Name = checkName
            };
        }

        public static IDatabaseCheckConstraint FromDto(this Dto.DatabaseCheckConstraint check)
        {
            if (check == null)
                throw new ArgumentNullException(nameof(check));

            var checkName = check.Name.FromDto();
            return new DatabaseCheckConstraint(
                checkName,
                check.Definition,
                check.IsEnabled
            );
        }
    }
}
