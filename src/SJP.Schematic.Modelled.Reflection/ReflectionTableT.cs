using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionTable<T> : ReflectionTable where T : class, new()
    {
        public ReflectionTable(IRelationalDatabase database)
            : base(database, typeof(T))
        {
        }
    }
}
