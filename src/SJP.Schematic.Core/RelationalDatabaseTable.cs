using System;
using System.Collections.Generic;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class RelationalDatabaseTable : IRelationalDatabaseTable
    {
        public RelationalDatabaseTable(
            Identifier tableName,
            IReadOnlyList<IDatabaseColumn> columns,
            Option<IDatabaseKey> primaryKey,
            IReadOnlyCollection<IDatabaseKey> uniqueKeys,
            IReadOnlyCollection<IDatabaseRelationalKey> parentKeys,
            IReadOnlyCollection<IDatabaseRelationalKey> childKeys,
            IReadOnlyCollection<IDatabaseIndex> indexes,
            IReadOnlyCollection<IDatabaseCheckConstraint> checks,
            IReadOnlyCollection<IDatabaseTrigger> triggers)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (uniqueKeys == null || uniqueKeys.AnyNull())
                throw new ArgumentNullException(nameof(uniqueKeys));
            if (parentKeys == null || parentKeys.AnyNull())
                throw new ArgumentNullException(nameof(parentKeys));
            if (childKeys == null || childKeys.AnyNull())
                throw new ArgumentNullException(nameof(childKeys));
            if (indexes == null || indexes.AnyNull())
                throw new ArgumentNullException(nameof(indexes));
            if (checks == null || checks.AnyNull())
                throw new ArgumentNullException(nameof(checks));
            if (triggers == null || triggers.AnyNull())
                throw new ArgumentNullException(nameof(triggers));

            Name = tableName ?? throw new ArgumentNullException(nameof(tableName));
            Columns = columns;
            PrimaryKey = primaryKey;
            UniqueKeys = uniqueKeys;
            ParentKeys = parentKeys;
            ChildKeys = childKeys;
            Indexes = indexes;
            Checks = checks;
            Triggers = triggers;
        }

        public Identifier Name { get; }

        public Option<IDatabaseKey> PrimaryKey { get; }

        public IReadOnlyCollection<IDatabaseIndex> Indexes { get; }

        public IReadOnlyCollection<IDatabaseKey> UniqueKeys { get; }

        public IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys { get; }

        public IReadOnlyCollection<IDatabaseCheckConstraint> Checks { get; }

        public IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys { get; }

        public IReadOnlyList<IDatabaseColumn> Columns { get; }

        public IReadOnlyCollection<IDatabaseTrigger> Triggers { get; }

        public override string ToString() => "Table: " + Name.ToString();

        private string DebuggerDisplay
        {
            get
            {
                var builder = StringBuilderCache.Acquire();

                builder.Append("Table: ");

                if (!Name.Schema.IsNullOrWhiteSpace())
                    builder.Append(Name.Schema).Append(".");

                builder.Append(Name.LocalName);

                return builder.GetStringAndRelease();
            }
        }
    }
}
