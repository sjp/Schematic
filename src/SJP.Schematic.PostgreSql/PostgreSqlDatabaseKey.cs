using System;
using System.Collections.Generic;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlDatabaseKey : IDatabaseKey
    {
        public PostgreSqlDatabaseKey(Identifier name, DatabaseKeyType keyType, IReadOnlyCollection<IDatabaseColumn> columns)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

            Name = Option<Identifier>.Some(name.LocalName);
            KeyType = keyType;
            Columns = columns;
        }

        public Option<Identifier> Name { get; }

        public DatabaseKeyType KeyType { get; }

        public IReadOnlyCollection<IDatabaseColumn> Columns { get; }

        public bool IsEnabled { get; } = true;
    }
}
