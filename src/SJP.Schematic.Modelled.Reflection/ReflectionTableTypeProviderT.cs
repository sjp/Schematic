using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionTableTypeProvider<T> : ReflectionTableTypeProvider where T : new()
    {
        public ReflectionTableTypeProvider(IDatabaseDialect dialect)
            : base(dialect, typeof(T))
        {
        }

        public new T TableInstance => (T)base.TableInstance;
    }
}
