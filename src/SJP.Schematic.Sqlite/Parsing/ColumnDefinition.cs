using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core.Extensions;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing
{
    internal class ColumnDefinition
    {
        public ColumnDefinition(SqlIdentifier columnName)
            : this(columnName.Value.LocalName, Enumerable.Empty<Token<SqliteToken>>(), Enumerable.Empty<ColumnConstraint>())
        {
        }

        public ColumnDefinition(SqlIdentifier columnName, IEnumerable<Token<SqliteToken>> typeDefinition)
            : this(columnName.Value.LocalName, typeDefinition, Enumerable.Empty<ColumnConstraint>())
        {
        }

        public ColumnDefinition(string columnName, IEnumerable<Token<SqliteToken>> typeDefinition, IEnumerable<ColumnConstraint> columnConstraints)
        {
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            Name = columnName;
            TypeDefinition = typeDefinition?.ToList() ?? Enumerable.Empty<Token<SqliteToken>>();

            var nullable = true;
            var autoIncrement = false;
            var collation = SqliteCollation.None;
            var defaultValue = new List<Token<SqliteToken>>();
            PrimaryKey primaryKey = null;
            UniqueKey uniqueKey = null;
            var foreignKeys = new List<ForeignKey>();
            var checks = new List<Check>();

            columnConstraints = columnConstraints?.ToList() ?? Enumerable.Empty<ColumnConstraint>();
            foreach (var constraint in columnConstraints)
            {
                switch (constraint.ConstraintType)
                {
                    case ColumnConstraint.ColumnConstraintType.Check:
                        var ck = constraint as ColumnConstraint.Check;
                        checks.Add(new Check(ck.Name, ck.Definition));
                        break;
                    case ColumnConstraint.ColumnConstraintType.Collation:
                        var col = constraint as ColumnConstraint.Collation;
                        collation = col.CollationType;
                        break;
                    case ColumnConstraint.ColumnConstraintType.Default:
                        var def = constraint as ColumnConstraint.DefaultConstraint;
                        defaultValue.AddRange(def.DefaultValue);
                        break;
                    case ColumnConstraint.ColumnConstraintType.ForeignKey:
                        var fk = constraint as ColumnConstraint.ForeignKey;
                        foreignKeys.Add(new ForeignKey(fk.Name, Name, fk.ParentTable, fk.ParentColumnNames));
                        break;
                    case ColumnConstraint.ColumnConstraintType.Nullable:
                        var nullableCons = constraint as ColumnConstraint.Nullable;
                        nullable = nullableCons.IsNullable;
                        break;
                    case ColumnConstraint.ColumnConstraintType.PrimaryKey:
                        var pk = constraint as ColumnConstraint.PrimaryKey;
                        autoIncrement = pk.AutoIncrement;
                        primaryKey = new PrimaryKey(pk.Name, new IndexedColumn(Name).WithColumnOrder(pk.ColumnOrder).ToEnumerable());
                        break;
                    case ColumnConstraint.ColumnConstraintType.UniqueKey:
                        var uk = constraint as ColumnConstraint.UniqueKey;
                        uniqueKey = new UniqueKey(uk.Name, Name);
                        break;
                }
            }

            if (primaryKey != null && uniqueKey != null)
                uniqueKey = null; // prefer primary key to unique key

            Nullable = nullable;
            IsAutoIncrement = autoIncrement;
            Collation = collation;
            DefaultValue = defaultValue;
            PrimaryKey = primaryKey;
            UniqueKey = uniqueKey;
            ForeignKeys = foreignKeys;
            Checks = checks;
        }

        public string Name { get; }

        public IEnumerable<Token<SqliteToken>> TypeDefinition { get; }

        public IEnumerable<Token<SqliteToken>> DefaultValue { get; }

        public bool Nullable { get; }

        public bool IsAutoIncrement { get; }

        public SqliteCollation Collation { get; }

        public PrimaryKey PrimaryKey { get; }

        public UniqueKey UniqueKey { get; }

        public IEnumerable<ForeignKey> ForeignKeys { get; }

        public IEnumerable<Check> Checks { get; }
    }
}
