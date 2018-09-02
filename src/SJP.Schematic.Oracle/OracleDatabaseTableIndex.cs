using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public class OracleDatabaseTableIndex : OracleDatabaseIndex<IRelationalDatabaseTable>, IDatabaseTableIndex
    {
        public OracleDatabaseTableIndex(IRelationalDatabaseTable table, Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, OracleIndexProperties properties)
            : base(table, name, isUnique, columns, properties)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }
}
