using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDatabaseTableIndex : SqlServerDatabaseIndex<IRelationalDatabaseTable>, IDatabaseTableIndex
    {
        public SqlServerDatabaseTableIndex(IRelationalDatabaseTable table, Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, IReadOnlyCollection<IDatabaseTableColumn> includedColumns, bool isEnabled)
            : base(table, name, isUnique, columns, includedColumns, isEnabled)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }
}
