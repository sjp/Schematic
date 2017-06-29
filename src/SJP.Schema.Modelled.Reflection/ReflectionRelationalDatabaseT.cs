using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    // TODO: uncomment interface when ready
    public class ReflectionRelationalDatabase<T> : ReflectionRelationalDatabase //, IDependentRelationalDatabase
    {
        public ReflectionRelationalDatabase(IDatabaseDialect dialect, string databaseName = null, string defaultSchema = null)
            : base(dialect, typeof(T), databaseName, defaultSchema)
        {
        }
    }
}
