using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql
{
    public class MySqlDatabaseTableIndex : MySqlDatabaseIndex<IRelationalDatabaseTable>, IDatabaseTableIndex
    {
        public MySqlDatabaseTableIndex(IRelationalDatabaseTable table, Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns)
            : base(table, name, isUnique, columns)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }
}
