using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class DatabaseMaterializedView : DatabaseView
    {
        public DatabaseMaterializedView(
            Identifier viewName,
            string definition,
            IReadOnlyList<IDatabaseColumn> columns
        ) : base(viewName, definition, columns)
        {
        }

        public override bool IsMaterialized { get; } = true;

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

                builder.Append("Materialized View: ");

                if (!Name.Schema.IsNullOrWhiteSpace())
                    builder.Append(Name.Schema).Append('.');

                builder.Append(Name.LocalName);

                return builder.GetStringAndRelease();
            }
        }
    }
}
