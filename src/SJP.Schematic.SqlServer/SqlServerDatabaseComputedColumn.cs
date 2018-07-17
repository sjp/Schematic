using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDatabaseComputedTableColumn : SqlServerDatabaseTableColumn, IDatabaseComputedColumn
    {
        public SqlServerDatabaseComputedTableColumn(IRelationalDatabaseTable table, Identifier columnName, IDbType type, bool isNullable, string defaultValue, string definition)
            : base(table, columnName, type, isNullable, defaultValue, null)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Definition = definition;
        }

        public string Definition { get; }

        public override bool IsComputed { get; } = true;
    }
}
