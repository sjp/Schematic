using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionView<T> : ReflectionView where T : class, new()
    {
        public ReflectionView(IRelationalDatabase database, IDatabaseDialect dialect)
            : base(database, dialect, typeof(T))
        {
        }
    }
}
