using System;
using System.Collections.Generic;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class RelationalDatabaseView : IRelationalDatabaseView
    {
        public RelationalDatabaseView(
            Identifier viewName,
            string definition,
            IReadOnlyList<IDatabaseColumn> columns,
            IReadOnlyCollection<IDatabaseIndex> indexes)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));
            if (columns == null || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (indexes == null || indexes.AnyNull())
                throw new ArgumentNullException(nameof(indexes));

            Name = viewName ?? throw new ArgumentNullException(nameof(viewName));
            Columns = columns;
            Indexes = indexes;
            IsIndexed = indexes.Count > 0;
            Definition = definition;
        }

        public Identifier Name { get; }

        public string Definition { get; }

        public bool IsIndexed { get; }

        public IReadOnlyCollection<IDatabaseIndex> Indexes { get; }

        public IReadOnlyList<IDatabaseColumn> Columns { get; }

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
