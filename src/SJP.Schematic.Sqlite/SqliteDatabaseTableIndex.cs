using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite
{
    public class SqliteDatabaseTableIndex : SqliteDatabaseIndex<IRelationalDatabaseTable>, IDatabaseTableIndex
    {
        public SqliteDatabaseTableIndex(IRelationalDatabaseTable table, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns, IEnumerable<IDatabaseColumn> includedColumns)
            : base(table, name, isUnique, columns, includedColumns)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }
}
