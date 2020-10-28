using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
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

        public Option<string> Name { get; protected set; }

        public TableConstraintType ConstraintType { get; }

        internal TableConstraint WithName(SqlIdentifier constraintName)
        {
            if (constraintName == null)
                throw new ArgumentNullException(nameof(constraintName));

            Name = Option<string>.Some(constraintName.Value.LocalName);
            return this;
        }

        public sealed class PrimaryKey : TableConstraint
        {
            public PrimaryKey(IReadOnlyCollection<IndexedColumn> columns)
                : base(TableConstraintType.PrimaryKey)
            {
                if (columns == null || columns.Empty())
                    throw new ArgumentNullException(nameof(columns));

                Columns = columns;
            }

            public IReadOnlyCollection<IndexedColumn> Columns { get; }
        }

        public sealed class UniqueKey : TableConstraint
        {
            public UniqueKey(IReadOnlyCollection<IndexedColumn> columns)
                : base(TableConstraintType.UniqueKey)
            {
                if (columns == null || columns.Empty())
                    throw new ArgumentNullException(nameof(columns));

                Columns = columns;
            }

            public IReadOnlyCollection<IndexedColumn> Columns { get; }
        }

        public sealed class ForeignKey : TableConstraint
        {
            public ForeignKey(IReadOnlyCollection<string> columnNames, Identifier parentTableName, IReadOnlyCollection<string> parentColumnNames)
                : base(TableConstraintType.ForeignKey)
            {
                if (columnNames == null || columnNames.Empty())
                    throw new ArgumentNullException(nameof(columnNames));
                if (parentColumnNames == null || parentColumnNames.Empty())
                    throw new ArgumentNullException(nameof(parentColumnNames));

                Columns = columnNames;
                ParentTable = parentTableName ?? throw new ArgumentNullException(nameof(parentTableName));
                ParentColumnNames = parentColumnNames;
            }

            public IReadOnlyCollection<string> Columns { get; }

            public Identifier ParentTable { get; }

            public IReadOnlyCollection<string> ParentColumnNames { get; }
        }

        public sealed class Check : TableConstraint
        {
            public Check(SqlExpression definition)
                : base(TableConstraintType.Check)
            {
                if (definition == null || definition.Tokens.Empty())
                    throw new ArgumentNullException(nameof(definition));

                Definition = definition.Tokens.ToList();
            }

            public IReadOnlyCollection<Token<SqliteToken>> Definition { get; }
        }
    }
}
