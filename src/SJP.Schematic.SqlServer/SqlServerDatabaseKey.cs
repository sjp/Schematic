using System;
using System.Collections.Generic;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDatabaseKey : IDatabaseKey
    {
        public SqlServerDatabaseKey(IRelationalDatabaseTable table, Identifier name, DatabaseKeyType keyType, IEnumerable<IDatabaseColumn> columns, bool isEnabled)
        {
            if (name == null || name.LocalName == null)
                throw new ArgumentNullException(nameof(name));
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

            Table = table ?? throw new ArgumentNullException(nameof(table));
            Name = name.LocalName;
            KeyType = keyType;
            Columns = columns;
            IsEnabled = isEnabled;
        }

        public IRelationalDatabaseTable Table { get; }

        public Identifier Name { get; }

        public DatabaseKeyType KeyType { get; }

        public IEnumerable<IDatabaseColumn> Columns { get; }

        public bool IsEnabled { get; }
    }
}
