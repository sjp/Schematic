using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionRoutine<T> : ReflectionRoutine where T : class, new()
    {
        public ReflectionRoutine(IRelationalDatabase database)
            : base(database, typeof(T))
        {
        }
    }
}
