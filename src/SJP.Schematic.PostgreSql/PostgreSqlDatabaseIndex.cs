using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql
{
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
    }
}
