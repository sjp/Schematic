using System;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public class DatabaseComputedColumn : DatabaseColumn, IDatabaseComputedColumn
    {
        public DatabaseComputedColumn(
            Identifier columnName,
            IDbType type,
            bool isNullable,
            Option<string> defaultValue,
            Option<string> definition
        ) : base(columnName, type, isNullable, defaultValue, Option<IAutoIncrement>.None)
        {
            Definition = definition;
        }

        public Option<string> Definition { get; }

        public override bool IsComputed { get; } = true;
    }
}
