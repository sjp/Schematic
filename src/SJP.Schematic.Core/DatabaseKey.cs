using System;
using System.Collections.Generic;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    public class DatabaseKey : IDatabaseKey
    {
        public DatabaseKey(Option<Identifier> name, DatabaseKeyType keyType, IReadOnlyCollection<IDatabaseColumn> columns, bool isEnabled)
        {
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

            Name = name.Map(n => Identifier.CreateQualifiedIdentifier(n.LocalName)); // strip to localname only
            KeyType = keyType;
            Columns = columns;
            IsEnabled = isEnabled;
        }

        public Option<Identifier> Name { get; }

        public DatabaseKeyType KeyType { get; }

        public IReadOnlyCollection<IDatabaseColumn> Columns { get; }

        public bool IsEnabled { get; }
    }
}
