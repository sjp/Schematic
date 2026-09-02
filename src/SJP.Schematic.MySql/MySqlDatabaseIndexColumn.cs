using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.MySql;

/// <summary>
/// A MySQL definition of an index column.
/// </summary>
/// <seealso cref="IDatabaseIndexColumn" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class MySqlDatabaseIndexColumn : IDatabaseIndexColumn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDatabaseIndexColumn"/> class.
    /// </summary>
    /// <param name="expression">An expression that represents the index column.</param>
    /// <param name="column">A database column the index is dependent on.</param>
    /// <exception cref="ArgumentNullException"><paramref name="column"/> or <paramref name="expression"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> is empty or whitespace.</exception>
    public MySqlDatabaseIndexColumn(string expression, IDatabaseColumn column)
        : this(expression, column, IndexColumnOrder.Ascending, Option<int>.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDatabaseIndexColumn"/> class.
    /// </summary>
    /// <param name="expression">An expression that represents the index column.</param>
    /// <param name="column">A database column the index is dependent on.</param>
    /// <param name="order">The sorting order applied to the index column.</param>
    /// <param name="prefixLength">The number of leading characters or bytes indexed, for a prefix index.</param>
    /// <exception cref="ArgumentNullException"><paramref name="column"/> or <paramref name="expression"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> is empty or whitespace, or <paramref name="order"/> is an invalid enum value.</exception>
    public MySqlDatabaseIndexColumn(string expression, IDatabaseColumn column, IndexColumnOrder order, Option<int> prefixLength)
    {
        ArgumentNullException.ThrowIfNull(column);
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        if (!order.IsValid())
            throw new ArgumentException($"The {nameof(IndexColumnOrder)} provided must be a valid enum.", nameof(order));

        Expression = expression;
        DependentColumns = [column];
        Order = order;
        PrefixLength = prefixLength;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDatabaseIndexColumn"/> class for a functional index column,
    /// i.e. one defined by an expression rather than by a column.
    /// </summary>
    /// <param name="expression">An expression that represents the index column.</param>
    /// <param name="order">The sorting order applied to the index column.</param>
    /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> is empty or whitespace, or <paramref name="order"/> is an invalid enum value.</exception>
    public MySqlDatabaseIndexColumn(string expression, IndexColumnOrder order)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        if (!order.IsValid())
            throw new ArgumentException($"The {nameof(IndexColumnOrder)} provided must be a valid enum.", nameof(order));

        Expression = expression;
        DependentColumns = [];
        Order = order;
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
    /// <value>Always <see cref="IndexColumnNullOrder.Default"/>. MySQL always sorts nulls first
    /// in an ascending index and last in a descending one.</value>
    public IndexColumnNullOrder NullOrder { get; } = IndexColumnNullOrder.Default;

    /// <summary>
    /// The collation applied to the index column.
    /// </summary>
    /// <value>Always 'none'. A MySQL index column always uses the collation of its column.</value>
    public Option<Identifier> Collation { get; } = Option<Identifier>.None;

    /// <summary>
    /// The number of leading characters or bytes of the column that are indexed, for a prefix index.
    /// </summary>
    /// <value>A prefix length, if the index covers only a prefix of the column's values.</value>
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