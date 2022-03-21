using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing;

/// <summary>
/// The parsed definition of an index column in a SQLite <c>CREATE TABLE</c> definition.
/// </summary>
public class IndexedColumn
{
    internal IndexedColumn(SqlIdentifier identifier)
    {
        if (identifier == null)
            throw new ArgumentNullException(nameof(identifier));

        Name = identifier.Value.LocalName;
        Expression = Array.Empty<Token<SqliteToken>>();
    }

    internal IndexedColumn(string identifier)
    {
        if (identifier.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(identifier));

        Name = identifier;
        Expression = Array.Empty<Token<SqliteToken>>();
    }

    internal IndexedColumn(SqlExpression expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        Expression = expression.Tokens.ToList();
    }

    internal IndexedColumn(IReadOnlyCollection<Token<SqliteToken>> expression)
    {
        if (expression == null || expression.Empty())
            throw new ArgumentNullException(nameof(expression));

        Expression = expression;
    }

    /// <summary>
    /// The indexed column name.
    /// </summary>
    /// <value>The indexed column name.</value>
    public string? Name { get; protected set; }

    /// <summary>
    /// A SQL expression representing the indexing on a column.
    /// </summary>
    /// <value>A SQL expression.</value>
    public IReadOnlyCollection<Token<SqliteToken>> Expression { get; protected set; }

    /// <summary>
    /// Indexing collation.
    /// </summary>
    /// <value>A collation.</value>
    public SqliteCollation Collation { get; protected set; }

    /// <summary>
    /// The column ordering for indexing.
    /// </summary>
    /// <value>A column order.</value>
    public IndexColumnOrder ColumnOrder { get; protected set; }

    internal IndexedColumn WithCollation(ColumnConstraint constraint)
    {
        if (constraint == null)
            throw new ArgumentNullException(nameof(constraint));
        if (constraint.ConstraintType != ColumnConstraint.ColumnConstraintType.Collation)
            throw new ArgumentException("The given column constraint is not collation constraint. Instead given: " + constraint.ConstraintType.ToString(), nameof(constraint));
        if (constraint is not ColumnConstraint.Collation collationConstraint)
            throw new ArgumentException("The given constraint does not match the given constraint type.", nameof(constraint));

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