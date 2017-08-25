using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schematic.Core
{
    public interface IDatabaseKey : IDatabaseOptional
    {
        IRelationalDatabaseTable Table { get; }

        Identifier Name { get; }

        IEnumerable<IDatabaseColumn> Columns { get; }

        DatabaseKeyType KeyType { get; }
    }
}
