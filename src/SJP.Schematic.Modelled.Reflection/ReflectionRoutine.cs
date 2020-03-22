using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionRoutine : IDatabaseRoutine
    {
        public ReflectionRoutine(IRelationalDatabase database, IDatabaseDialect dialect, Type routineType)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));

            RoutineType = routineType ?? throw new ArgumentNullException(nameof(routineType));
            Name = dialect.GetQualifiedNameOrDefault(database, RoutineType);
        }

        protected Type RoutineType { get; }

        public string Definition => throw new NotImplementedException();

        public Identifier Name { get; }
    }
}
