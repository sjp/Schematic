using System;
using System.Collections.Generic;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite
{
    public class SqliteDatabaseKey : IDatabaseKey
    {
        public SqliteDatabaseKey(IRelationalDatabaseTable table, Identifier name, DatabaseKeyType keyType, IEnumerable<IDatabaseColumn> columns)
        {
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

            if (name?.LocalName != null)
                Name = name.LocalName; // can be null!

            Table = table ?? throw new ArgumentNullException(nameof(table));
            KeyType = keyType;
            Columns = columns;
        }

        public IRelationalDatabaseTable Table { get; }

        public Identifier Name { get; }

        public DatabaseKeyType KeyType { get; }

        public IEnumerable<IDatabaseColumn> Columns { get; }

        public bool IsEnabled { get; } = true;
    }
}
