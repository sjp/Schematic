using System;
using System.Collections.Generic;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class DatabaseView : IDatabaseView
    {
        public DatabaseView(
            Identifier viewName,
            string definition,
            IReadOnlyList<IDatabaseColumn> columns
        )
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));
            if (columns == null || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));

            Name = viewName ?? throw new ArgumentNullException(nameof(viewName));
            Columns = columns;
            Definition = definition;
        }

        public Identifier Name { get; }

        public string Definition { get; }

        public IReadOnlyList<IDatabaseColumn> Columns { get; }

        public virtual bool IsMaterialized { get; }

        public override string ToString() => "View: " + Name.ToString();

        private string DebuggerDisplay
        {
            get
            {
                var builder = StringBuilderCache.Acquire();

                builder.Append("View: ");

                if (!Name.Schema.IsNullOrWhiteSpace())
                    builder.Append(Name.Schema).Append(".");

                builder.Append(Name.LocalName);

                return builder.GetStringAndRelease();
            }
        }
    }
}
