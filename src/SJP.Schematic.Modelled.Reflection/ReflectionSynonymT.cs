using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionSynonym<T> : ReflectionSynonym where T : class, new()
    {
        public ReflectionSynonym(IRelationalDatabase database)
            : base(database, typeof(T))
        {
        }
    }
}
