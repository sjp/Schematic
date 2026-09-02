using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.PostgreSql;

// TODO: remove this when the dependent columns can be parsed out

/// <summary>
/// A PostgreSQL definition of an index column.
/// </summary>
/// <seealso cref="IDatabaseIndexColumn" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class PostgreSqlDatabaseIndexColumn : IDatabaseIndexColumn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDatabaseIndexColumn"/> class.
    /// </summary>
    /// <param name="expression">An expression that represents the index column.</param>
    /// <param name="order">The sorting order applied to the index column.</param>
    /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> is empty or whitespace, or <paramref name="order"/> is an invalid enum value.</exception>
    public PostgreSqlDatabaseIndexColumn(string expression, IndexColumnOrder order)
        : this(expression, order, IndexColumnNullOrder.Default, Option<Identifier>.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDatabaseIndexColumn"/> class.
    /// </summary>
    /// <param name="expression">An expression that represents the index column.</param>
    /// <param name="order">The sorting order applied to the index column.</param>
    /// <param name="nullOrder">Where null values sort relative to non-null values.</param>
    /// <param name="collation">The collation applied to the index column, if one is reported.</param>
    /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> is empty or whitespace, or <paramref name="order"/> or <paramref name="nullOrder"/> is an invalid enum value.</exception>
    public PostgreSqlDatabaseIndexColumn(string expression, IndexColumnOrder order, IndexColumnNullOrder nullOrder, Option<Identifier> collation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        if (!order.IsValid())
            throw new ArgumentException($"The {nameof(IndexColumnOrder)} provided must be a valid enum.", nameof(order));
        if (!nullOrder.IsValid())
            throw new ArgumentException($"The {nameof(IndexColumnNullOrder)} provided must be a valid enum.", nameof(nullOrder));

        Expression = expression;
        Order = order;
        NullOrder = nullOrder;
        Collation = collation;
        DependentColumns = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDatabaseIndexColumn"/> class.
    /// </summary>
    /// <param name="expression">An expression that represents the index column.</param>
    /// <param name="column">A database column the index is dependent on.</param>
    /// <param name="order">The sorting order applied to the index column.</param>
    /// <exception cref="ArgumentNullException"><paramref name="column"/> or <paramref name="expression"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> is empty or whitespace, or <paramref name="order"/> is an invalid enum value.</exception>
    public PostgreSqlDatabaseIndexColumn(string expression, IDatabaseColumn column, IndexColumnOrder order)
        : this(expression, column, order, IndexColumnNullOrder.Default, Option<Identifier>.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDatabaseIndexColumn"/> class.
    /// </summary>
    /// <param name="expression">An expression that represents the index column.</param>
    /// <param name="column">A database column the index is dependent on.</param>
    /// <param name="order">The sorting order applied to the index column.</param>
    /// <param name="nullOrder">Where null values sort relative to non-null values.</param>
    /// <param name="collation">The collation applied to the index column, if one is reported.</param>
    /// <exception cref="ArgumentNullException"><paramref name="column"/> or <paramref name="expression"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> is empty or whitespace, or <paramref name="order"/> or <paramref name="nullOrder"/> is an invalid enum value.</exception>
    public PostgreSqlDatabaseIndexColumn(string expression, IDatabaseColumn column, IndexColumnOrder order, IndexColumnNullOrder nullOrder, Option<Identifier> collation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        ArgumentNullException.ThrowIfNull(column);
        if (!order.IsValid())
            throw new ArgumentException($"The {nameof(IndexColumnOrder)} provided must be a valid enum.", nameof(order));
        if (!nullOrder.IsValid())
            throw new ArgumentException($"The {nameof(IndexColumnNullOrder)} provided must be a valid enum.", nameof(nullOrder));

        Expression = expression;
        DependentColumns = [column];
        Order = order;
        NullOrder = nullOrder;
        Collation = collation;
    }

    /// <summary>
    /// An expression that represents the given index column e.g. <c>UPPER(name)</c>.
    /// </summary>
    /// <value>A textual expression.</value>
    public string Expression { get; }

    /// <summary>
    /// The set of columns that the index column is dependent upon.
    /// </summary>
    /// <value>The dependent columns.</value>
    public IReadOnlyList<IDatabaseColumn> DependentColumns { get; }

    /// <summary>
    /// The ordering applied to the column.
    /// </summary>
    /// <value>The ordering.</value>
    public IndexColumnOrder Order { get; }

    /// <summary>
    /// Where null values sort relative to non-null values within the column.
    /// </summary>
    /// <value>A null ordering.</value>
    public IndexColumnNullOrder NullOrder { get; }

    /// <summary>
    /// The collation applied to the index column.
    /// </summary>
    /// <value>A collation name, if the index column declares one.</value>
    public Option<Identifier> Collation { get; }

    /// <summary>
    /// The number of leading characters or bytes of the column that are indexed, for a prefix index.
    /// </summary>
    /// <value>Always 'none'. PostgreSQL has no prefix indexes.</value>
    public Option<int> PrefixLength { get; } = Option<int>.None;

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => DebuggerDisplay;

    private string DebuggerDisplay
    {
        get
        {
            var builder = StringBuilderCache.Acquire();

            builder.Append("Index Column: ")
                .Append(Expression);

            return builder.GetStringAndRelease();
        }
    }
}