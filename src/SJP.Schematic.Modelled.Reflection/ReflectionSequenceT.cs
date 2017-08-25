using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionSequence<T> : ReflectionSequence where T : ISequence, new()
    {
        public ReflectionSequence(IRelationalDatabase database)
            : base(database, typeof(T))
        {
        }
    }
}
