using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    public class DatabaseKey : IDatabaseKey
    {
        public DatabaseKey(IRelationalDatabaseTable table, Identifier name, DatabaseKeyType keyType, IEnumerable<IDatabaseColumn> columns, bool isEnabled)
        {
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

            Table = table ?? throw new ArgumentNullException(nameof(table));

            if (name?.LocalName != null)
                Name = name.LocalName; // can be null!

            KeyType = keyType;
            Columns = columns.ToList();
            IsEnabled = isEnabled;
        }

        public IRelationalDatabaseTable Table { get; }

        public Identifier Name { get; }

        public DatabaseKeyType KeyType { get; }

        public IReadOnlyCollection<IDatabaseColumn> Columns { get; }

        public bool IsEnabled { get; }
    }
}
