using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle
{
    public class OracleDatabaseComputedColumn : OracleDatabaseColumn, IDatabaseComputedColumn
    {
        public OracleDatabaseComputedColumn(Identifier columnName, IDbType type, bool isNullable, string definition)
            : base(columnName, type, isNullable, definition)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Definition = definition;
        }

        public string Definition { get; }

        public override bool IsComputed { get; } = true;
    }
}
