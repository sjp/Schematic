using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionTypeProvider<T> : ReflectionTypeProvider where T : new()
    {
        public ReflectionTypeProvider(IDatabaseDialect dialect)
            : base(dialect, typeof(T))
        {
        }
    }
}
