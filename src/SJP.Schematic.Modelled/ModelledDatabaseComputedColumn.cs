using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled
{
    public class ModelledDatabaseComputedTableColumn : ModelledDatabaseTableColumn, IDatabaseTableColumn, IDatabaseComputedColumn
    {
        public ModelledDatabaseComputedTableColumn(IRelationalDatabaseTable table, Identifier columnName, IDbType type, bool isNullable, string defaultValue, string definition)
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
