using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionRoutine<T> : ReflectionRoutine where T : class, new()
    {
        public ReflectionRoutine(IRelationalDatabase database, IDatabaseDialect dialect)
            : base(database, dialect, typeof(T))
        {
        }
    }
}
