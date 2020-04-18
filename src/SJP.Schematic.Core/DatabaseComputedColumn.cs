using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => DebuggerDisplay;

        private string DebuggerDisplay
        {
            get
            {
                var builder = StringBuilderCache.Acquire();

                builder.Append("Computed Column: ")
                    .Append(Name.LocalName);

                return builder.GetStringAndRelease();
            }
        }
    }
}
