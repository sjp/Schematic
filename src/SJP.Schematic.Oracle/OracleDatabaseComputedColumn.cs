using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Oracle
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class OracleDatabaseComputedColumn : OracleDatabaseColumn, IDatabaseComputedColumn
    {
        public OracleDatabaseComputedColumn(Identifier columnName, IDbType type, bool isNullable, Option<string> definition)
            : base(columnName, type, isNullable, definition)
        {
            Definition = definition;
        }

        public Option<string> Definition { get; }

        public override bool IsComputed { get; } = true;

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
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
