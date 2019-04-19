using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class DatabaseRoutineMappingExtensions
    {
        public static Dto.DatabaseRoutine ToDto(this IDatabaseRoutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            var routineName = routine.Name.ToDto();

            return new Dto.DatabaseRoutine
            {
                Name = routineName,
                Definition = routine.Definition
            };
        }

        public static IDatabaseRoutine FromDto(this Dto.DatabaseRoutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            var routineName = routine.Name.FromDto();
            return new DatabaseRoutine(
                (Identifier)routineName,
                routine.Definition
            );
        }
    }
}
