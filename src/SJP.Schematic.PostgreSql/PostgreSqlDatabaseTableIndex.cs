using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlDatabaseTableIndex : PostgreSqlDatabaseIndex<IRelationalDatabaseTable>, IDatabaseTableIndex
    {
        public PostgreSqlDatabaseTableIndex(IRelationalDatabaseTable table, Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns)
            : base(table, name, isUnique, columns)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }
}
