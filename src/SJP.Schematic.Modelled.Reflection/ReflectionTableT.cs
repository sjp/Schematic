using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionTable<T> : ReflectionTable where T : class, new()
    {
        public ReflectionTable(IRelationalDatabase database)
            : base(database, typeof(T))
        {
        }
    }
}
