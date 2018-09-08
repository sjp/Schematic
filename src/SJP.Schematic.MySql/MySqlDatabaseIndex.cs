using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql
{
    public abstract class MySqlDatabaseIndex<T> : IDatabaseIndex<T> where T : class, IDatabaseQueryable
    {
        protected MySqlDatabaseIndex(T parent, Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));

            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Name = name.LocalName;
            IsUnique = isUnique;
            Columns = columns;
        }

        public T Parent { get; }

        public Identifier Name { get; }

        public bool IsUnique { get; }

        public IReadOnlyCollection<IDatabaseIndexColumn> Columns { get; }

        public IReadOnlyCollection<IDatabaseColumn> IncludedColumns { get; } = Array.Empty<IDatabaseColumn>();

        public bool IsEnabled { get; } = true;
    }
}
