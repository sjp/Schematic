using System.Collections.Generic;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
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

        public override string ToString() => "Materialized View: " + Name.ToString();

        private string DebuggerDisplay
        {
            get
            {
                var builder = StringBuilderCache.Acquire();

                builder.Append("Materialized View: ");

                if (!Name.Schema.IsNullOrWhiteSpace())
                    builder.Append(Name.Schema).Append(".");

                builder.Append(Name.LocalName);

                return builder.GetStringAndRelease();
            }
        }
    }
}
