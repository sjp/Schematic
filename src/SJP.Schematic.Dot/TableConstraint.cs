using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Dot
{
    internal sealed class TableConstraint
    {
        public TableConstraint(
            string identifier,
            DatabaseKeyType keyType,
            string constraintName,
            IEnumerable<string> columnNames,
            IEnumerable<string> columnTypes
        )
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            Identifier = identifier;

            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

            KeyTitle = _keyTypeTitles[keyType];
            KeyType = keyType;

            ConstraintName = constraintName ?? string.Empty;

            ColumnNames = columnNames?.ToList() ?? throw new ArgumentNullException(nameof(columnNames));
            ColumnTypes = columnTypes?.ToList() ?? throw new ArgumentNullException(nameof(columnTypes));
        }

        public string Identifier { get; }

        public string KeyTitle { get; }

        public DatabaseKeyType KeyType { get; }

        public string ConstraintName { get; }

        public IReadOnlyList<string> ColumnNames { get; }

        public IReadOnlyList<string> ColumnTypes { get; }

        private static readonly IReadOnlyDictionary<DatabaseKeyType, string> _keyTypeTitles = new Dictionary<DatabaseKeyType, string>
        {
            [DatabaseKeyType.Foreign] = "Foreign Key",
            [DatabaseKeyType.Unique] = "Unique Key",
            [DatabaseKeyType.Primary] = "Primary Key"
        };
    }
}
