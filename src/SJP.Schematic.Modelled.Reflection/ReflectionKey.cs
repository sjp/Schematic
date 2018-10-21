using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionKey : DatabaseKey
    {
        public ReflectionKey(Identifier name, DatabaseKeyType keyType, IReadOnlyCollection<IDatabaseColumn> columns)
            : base(name, keyType, columns, true) // TODO: should we ever allow disabled keys?
        {
        }
    }
}
