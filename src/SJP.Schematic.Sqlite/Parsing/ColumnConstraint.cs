using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing
{
    internal abstract class ColumnConstraint
    {
        public enum ColumnConstraintType
        {
            Default,
            Collation,
            PrimaryKey,
            UniqueKey,
            ForeignKey,
            Check,
            Nullable
        }

        protected ColumnConstraint(ColumnConstraintType constraintType)
        {
            if (!constraintType.IsValid())
                throw new ArgumentException($"The { nameof(ColumnConstraintType) } provided must be a valid enum.", nameof(constraintType));

            ConstraintType = constraintType;
        }

        public string Name { get; protected set; }

        public ColumnConstraintType ConstraintType { get; }

        public ColumnConstraint WithName(SqlIdentifier identifier)
        {
            Name = identifier?.Value?.LocalName ?? throw new ArgumentNullException(nameof(identifier));
            return this;
        }

        public class Collation : ColumnConstraint
        {
            public Collation(Token<SqliteToken> collation)
                : base(ColumnConstraintType.Collation)
            {
                var collationName = collation.ToStringValue();

                CollationType = Enum.TryParse(collationName, out SqliteCollation type)
                    ? type
                    : SqliteCollation.None;
            }

            public SqliteCollation CollationType { get; }
        }

        public class Nullable : ColumnConstraint
        {
            public Nullable(bool isNullable)
                : base(ColumnConstraintType.Nullable)
            {
                IsNullable = isNullable;
            }

            public bool IsNullable { get; }
        }

        public class DefaultConstraint : ColumnConstraint
        {
            public DefaultConstraint(IEnumerable<Token<SqliteToken>> tokens)
                : base(ColumnConstraintType.Default)
            {
                if (tokens == null || tokens.Empty())
                    throw new ArgumentNullException(nameof(tokens));

                DefaultValue = tokens.ToList();
            }

            public IEnumerable<Token<SqliteToken>> DefaultValue { get; }
        }

        public class PrimaryKey : ColumnConstraint
        {
            public PrimaryKey(IndexColumnOrder columnOrder = IndexColumnOrder.Ascending, bool autoIncrement = false)
                : base(ColumnConstraintType.PrimaryKey)
            {
                if (!columnOrder.IsValid())
                    throw new ArgumentException($"The { nameof(IndexColumnOrder) } provided must be a valid enum.", nameof(columnOrder));

                ColumnOrder = columnOrder;
                AutoIncrement = autoIncrement;
            }

            public IndexColumnOrder ColumnOrder { get; }

            public bool AutoIncrement { get; }
        }

        public class UniqueKey : ColumnConstraint
        {
            public UniqueKey()
                : base(ColumnConstraintType.UniqueKey)
            {
            }
        }

        public class ForeignKey : ColumnConstraint
        {
            public ForeignKey(SqlIdentifier parentTableName, IEnumerable<SqlIdentifier> parentColumnNames)
                : base(ColumnConstraintType.ForeignKey)
            {
                if (parentTableName == null)
                    throw new ArgumentNullException(nameof(parentTableName));
                if (parentColumnNames == null || parentColumnNames.Empty())
                    throw new ArgumentNullException(nameof(parentColumnNames));

                ParentTable = parentTableName.Value;
                ParentColumnNames = parentColumnNames.Select(c => c.Value.LocalName).ToList();
            }

            public Identifier ParentTable { get; }

            public IEnumerable<string> ParentColumnNames { get; }
        }

        public class Check : ColumnConstraint
        {
            public Check(SqlExpression definition)
                : base(ColumnConstraintType.Check)
            {
                if (definition == null || definition.Tokens.Empty())
                    throw new ArgumentNullException(nameof(definition));

                Definition = definition.Tokens.ToList();
            }

            public IEnumerable<Token<SqliteToken>> Definition { get; }
        }
    }
}
