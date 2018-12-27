using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite
{
    public class SqliteDatabaseKey : IDatabaseKey
    {
        public SqliteDatabaseKey(Option<Identifier> name, DatabaseKeyType keyType, IEnumerable<IDatabaseColumn> columns)
        {
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

            Name = name.Map(n => new Identifier(n.LocalName));
            KeyType = keyType;
            Columns = columns.ToList();
        }

        public Option<Identifier> Name { get; }

        public DatabaseKeyType KeyType { get; }

        public IReadOnlyCollection<IDatabaseColumn> Columns { get; }

        public bool IsEnabled { get; } = true;
    }
}
