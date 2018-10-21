using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionRelationalDatabase<T> : ReflectionRelationalDatabase
    {
        public ReflectionRelationalDatabase(IDatabaseDialect dialect, string databaseName = null, string defaultSchema = null)
            : base(dialect, typeof(T), databaseName, defaultSchema)
        {
        }
    }
}
