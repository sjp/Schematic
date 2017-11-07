using System;
using System.Collections.Generic;
using EnumsNET;
using SJP.Schematic.Core;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class IndexedColumn
    {
        internal IndexedColumn(SqlIdentifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            Name = identifier.Value.LocalName;
        }

        internal IndexedColumn(string identifier)
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            Name = identifier;
        }

        internal IndexedColumn(SqlExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            Expression = expression.Tokens;
        }

        internal IndexedColumn(IEnumerable<Token<SqliteToken>> expression)
        {
            if (expression == null || expression.Empty())
                throw new ArgumentNullException(nameof(expression));

            Expression = expression;
        }

        public string Name { get; protected set; }

        public IEnumerable<Token<SqliteToken>> Expression { get; protected set; }

        public SqliteCollation Collation { get; protected set; }

        public IndexColumnOrder ColumnOrder { get; protected set; }

        internal IndexedColumn WithCollation(ColumnConstraint constraint)
        {
            if (constraint == null)
                throw new ArgumentNullException(nameof(constraint));
            if (constraint.ConstraintType != ColumnConstraint.ColumnConstraintType.Collation)
                throw new ArgumentException("The given column constraint is not collation constraint. Instead given: " + constraint.ConstraintType.ToString(), nameof(constraint));

            var collationConstraint = constraint as ColumnConstraint.Collation;
            var collation = collationConstraint.CollationType;

            var newColumn = Name != null
                ? new IndexedColumn(Name)
                : new IndexedColumn(Expression);
            newColumn.Collation = collation;
            newColumn.ColumnOrder = ColumnOrder;

            return newColumn;
        }

        internal IndexedColumn WithColumnOrder(IndexColumnOrder columnOrder)
        {
            if (!columnOrder.IsValid())
                throw new ArgumentException($"The { nameof(IndexColumnOrder) } provided must be a valid enum.", nameof(columnOrder));

            var newColumn = Name != null
                ? new IndexedColumn(Name)
                : new IndexedColumn(Expression);
            newColumn.Collation = Collation;
            newColumn.ColumnOrder = ColumnOrder;

            return newColumn;
        }
    }
}
