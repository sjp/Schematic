using System;
using SJP.Schema.Core;

namespace SJP.Schema.SqlServer
{
    public class SqlServerDatabaseComputedTableColumn : SqlServerDatabaseTableColumn, IDatabaseTableColumn, IDatabaseComputedColumn
    {
        public SqlServerDatabaseComputedTableColumn(IRelationalDatabaseTable table, Identifier columnName, IDbType type, bool isNullable, string defaultValue, string definition)
            : base(table, columnName, type, isNullable, defaultValue, false)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Definition = definition;
        }

        public string Definition { get; }

        public override bool IsComputed { get; } = true;
    }
}
