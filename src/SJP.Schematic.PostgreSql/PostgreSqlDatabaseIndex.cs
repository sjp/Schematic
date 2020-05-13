using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.PostgreSql
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class PostgreSqlDatabaseIndex : IDatabaseIndex
    {
        public PostgreSqlDatabaseIndex(Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns)
            : this(name, isUnique, columns, Array.Empty<IDatabaseColumn>())
        {
        }

        public PostgreSqlDatabaseIndex(Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, IReadOnlyCollection<IDatabaseColumn> includedColumns)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (includedColumns == null || includedColumns.AnyNull())
                throw new ArgumentNullException(nameof(includedColumns));

            Name = name.LocalName;
            IsUnique = isUnique;
            Columns = columns;
            IncludedColumns = includedColumns;
        }

        public Identifier Name { get; }

        public bool IsUnique { get; }

        public IReadOnlyCollection<IDatabaseIndexColumn> Columns { get; }

        public IReadOnlyCollection<IDatabaseColumn> IncludedColumns { get; }

        public bool IsEnabled { get; } = true;

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

                builder.Append("Index: ");

                if (!Name.Schema.IsNullOrWhiteSpace())
                    builder.Append(Name.Schema).Append('.');

                builder.Append(Name.LocalName);

                return builder.GetStringAndRelease();
            }
        }
    }
}
