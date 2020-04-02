using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core.Extensions;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class Column
    {
        public Column(
            string columnName,
            IEnumerable<Token<SqliteToken>> typeDefinition,
            bool nullable,
            bool autoIncrement,
            SqliteCollation collation,
            IEnumerable<Token<SqliteToken>> defaultValue,
            IEnumerable<Token<SqliteToken>> computedDefinition,
            SqliteGeneratedColumnType computedColumnType
        )
        {
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));
            if (!collation.IsValid())
                throw new ArgumentException($"The { nameof(SqliteCollation) } provided must be a valid enum.", nameof(collation));

            Name = columnName;
            TypeDefinition = typeDefinition?.ToList() ?? Enumerable.Empty<Token<SqliteToken>>();
            Nullable = nullable;
            IsAutoIncrement = autoIncrement;
            Collation = collation;
            DefaultValue = defaultValue?.ToList() ?? Enumerable.Empty<Token<SqliteToken>>();
            ComputedDefinition = computedDefinition?.ToList() ?? Enumerable.Empty<Token<SqliteToken>>();
            ComputedColumnType = computedColumnType;
        }

        public string Name { get; }

        public IEnumerable<Token<SqliteToken>> TypeDefinition { get; }

        public bool Nullable { get; }

        public bool IsAutoIncrement { get; }

        public SqliteCollation Collation { get; }

        public IEnumerable<Token<SqliteToken>> DefaultValue { get; }

        public IEnumerable<Token<SqliteToken>> ComputedDefinition { get; }

        public SqliteGeneratedColumnType ComputedColumnType { get; }
    }
}
