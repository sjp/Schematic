using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing
{
    internal abstract class TableConstraint
    {
        public enum TableConstraintType
        {
            PrimaryKey,
            UniqueKey,
            ForeignKey,
            Check
        }

        protected TableConstraint(TableConstraintType constraintType)
        {
            if (!constraintType.IsValid())
                throw new ArgumentException($"The { nameof(TableConstraintType) } provided must be a valid enum.", nameof(constraintType));

            ConstraintType = constraintType;
        }

        public string Name { get; protected set; }

        public TableConstraintType ConstraintType { get; }

        internal TableConstraint WithName(SqlIdentifier constraintName)
        {
            if (constraintName == null)
                throw new ArgumentNullException(nameof(constraintName));

            Name = constraintName.Value.LocalName;
            return this;
        }

        public class PrimaryKey : TableConstraint
        {
            public PrimaryKey(IEnumerable<IndexedColumn> columns)
                : base(TableConstraintType.PrimaryKey)
            {
                if (columns == null || columns.Empty())
                    throw new ArgumentNullException(nameof(columns));

                Columns = columns.ToList();
            }

            public IEnumerable<IndexedColumn> Columns { get; }
        }

        public class UniqueKey : TableConstraint
        {
            public UniqueKey(IEnumerable<IndexedColumn> columns)
                : base(TableConstraintType.UniqueKey)
            {
                if (columns == null || columns.Empty())
                    throw new ArgumentNullException(nameof(columns));

                Columns = columns.ToList();
            }

            public IEnumerable<IndexedColumn> Columns { get; }
        }

        public class ForeignKey : TableConstraint
        {
            public ForeignKey(IEnumerable<string> columnNames, Identifier parentTableName, IEnumerable<string> parentColumnNames)
                : base(TableConstraintType.ForeignKey)
            {
                if (columnNames == null || columnNames.Empty())
                    throw new ArgumentNullException(nameof(columnNames));
                if (parentColumnNames == null || parentColumnNames.Empty())
                    throw new ArgumentNullException(nameof(parentColumnNames));

                Columns = columnNames.ToList();
                ParentTable = parentTableName ?? throw new ArgumentNullException(nameof(parentTableName));
                ParentColumnNames = parentColumnNames.ToList();
            }

            public IEnumerable<string> Columns { get; }

            public Identifier ParentTable { get; }

            public IEnumerable<string> ParentColumnNames { get; }
        }

        public class Check : TableConstraint
        {
            public Check(SqlExpression definition)
                : base(TableConstraintType.Check)
            {
                if (definition == null || definition.Tokens.Empty())
                    throw new ArgumentNullException(nameof(definition));

                Definition = definition.Tokens.ToList();
            }

            public IEnumerable<Token<SqliteToken>> Definition { get; }
        }
    }
}
