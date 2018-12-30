using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public class OracleDatabaseComputedColumn : OracleDatabaseColumn, IDatabaseComputedColumn
    {
        public OracleDatabaseComputedColumn(Identifier columnName, IDbType type, bool isNullable, Option<string> definition)
            : base(columnName, type, isNullable, definition)
        {
            Definition = definition;
        }

        public Option<string> Definition { get; }

        public override bool IsComputed { get; } = true;
    }
}
