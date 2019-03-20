using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionRoutine : IDatabaseRoutine
    {
        public ReflectionRoutine(IRelationalDatabase database, Type routineType)
        {
            RoutineType = routineType ?? throw new ArgumentNullException(nameof(routineType));
            Name = database.Dialect.GetQualifiedNameOrDefault(database, RoutineType);
        }

        protected Type RoutineType { get; }

        public string Definition => throw new NotImplementedException();

        public Identifier Name { get; }
    }
}
