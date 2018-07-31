using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionKey : DatabaseKey
    {
        public ReflectionKey(IRelationalDatabaseTable table, Identifier name, DatabaseKeyType keyType, IReadOnlyCollection<IDatabaseColumn> columns)
            : base(table, name, keyType, columns, true) // TODO: should we ever allow disabled keys?
        {
        }
    }
}
