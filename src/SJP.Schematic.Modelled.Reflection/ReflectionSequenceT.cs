using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionSequence<T> : ReflectionSequence where T : ISequence, new()
    {
        public ReflectionSequence(IRelationalDatabase database, IDatabaseDialect dialect)
            : base(database, dialect, typeof(T))
        {
        }
    }
}
