using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql
{
    public class MySqlDatabaseComputedTableColumn : MySqlDatabaseTableColumn, IDatabaseTableColumn, IDatabaseComputedColumn
    {
        public MySqlDatabaseComputedTableColumn(IRelationalDatabaseTable table, Identifier columnName, IDbType type, bool isNullable, string defaultValue, string definition)
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
