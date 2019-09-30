using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionRelationalDatabase<T> : ReflectionRelationalDatabase
    {
        public ReflectionRelationalDatabase(IDatabaseDialect dialect, IIdentifierDefaults identifierDefaults)
            : base(dialect, typeof(T), identifierDefaults)
        {
        }
    }
}
