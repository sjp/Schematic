using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A database index column.
/// </summary>
/// <seealso cref="IDatabaseIndexColumn" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseIndexColumn : IDatabaseIndexColumn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseIndexColumn"/> class.
    /// </summary>
    /// <param name="expression">A textual expression defining the index column.</param>
    /// <param name="column">A column that the index column is dependent upon.</param>
    /// <param name="order">The index column ordering.</param>
    /// <exception cref="ArgumentNullException"><paramref name="column"/> or <paramref name="expression"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> is empty or whitespace, or <paramref name="order"/> is an invalid enum.</exception>
    public DatabaseIndexColumn(string expression, IDatabaseColumn column, IndexColumnOrder order)
        : this(expression, [column], order)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseIndexColumn"/> class.
    /// </summary>
    /// <param name="expression">A textual expression defining the index column.</param>
    /// <param name="column">A column that the index column is dependent upon.</param>
    /// <param name="order">The index column ordering.</param>
    /// <param name="nullOrder">Where null values sort relative to non-null values.</param>
    /// <param name="collation">The collation applied to the index column, if the database reports one.</param>
    /// <param name="prefixLength">The number of leading characters or bytes indexed, for a prefix index.</param>
    /// <exception cref="ArgumentNullException"><paramref name="column"/> or <paramref name="expression"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> is empty or whitespace, or <paramref name="order"/> or <paramref name="nullOrder"/> is an invalid enum.</exception>
    public DatabaseIndexColumn(string expression, IDatabaseColumn column, IndexColumnOrder order, IndexColumnNullOrder nullOrder, Option<Identifier> collation, Option<int> prefixLength)
        : this(expression, [column], order, nullOrder, collation, prefixLength)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseIndexColumn"/> class.
    /// </summary>
    /// <param name="expression">A textual expression defining the index column.</param>
    /// <param name="dependentColumns">Columns that the index column is dependent upon.</param>
    /// <param name="order">The index column ordering.</param>
    /// <exception cref="ArgumentNullException"><paramref name="dependentColumns"/> is <see langword="null" /> or contains <see langword="null" /> values, or <paramref name="expression"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> is empty or whitespace, <paramref name="dependentColumns"/> is empty, or <paramref name="order"/> is an invalid enum.</exception>
    public DatabaseIndexColumn(string expression, IEnumerable<IDatabaseColumn> dependentColumns, IndexColumnOrder order)
        : this(expression, dependentColumns, order, IndexColumnNullOrder.Default, Option<Identifier>.None, Option<int>.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseIndexColumn"/> class.
    /// </summary>
    /// <param name="expression">A textual expression defining the index column.</param>
    /// <param name="dependentColumns">Columns that the index column is dependent upon.</param>
    /// <param name="order">The index column ordering.</param>
    /// <param name="nullOrder">Where null values sort relative to non-null values.</param>
    /// <param name="collation">The collation applied to the index column, if the database reports one.</param>
    /// <param name="prefixLength">The number of leading characters or bytes indexed, for a prefix index.</param>
    /// <exception cref="ArgumentNullException"><paramref name="dependentColumns"/> is <see langword="null" /> or contains <see langword="null" /> values, or <paramref name="expression"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> is empty or whitespace, <paramref name="dependentColumns"/> is empty, or <paramref name="order"/> or <paramref name="nullOrder"/> is an invalid enum.</exception>
    public DatabaseIndexColumn(string expression, IEnumerable<IDatabaseColumn> dependentColumns, IndexColumnOrder order, IndexColumnNullOrder nullOrder, Option<Identifier> collation, Option<int> prefixLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);

        var columns = dependentColumns.ToDefensiveCopy(nameof(dependentColumns));
        if (columns.Empty())
            throw new ArgumentException("An index column must depend on at least one column.", nameof(dependentColumns));
        if (!order.IsValid())
            throw new ArgumentException($"The {nameof(IndexColumnOrder)} provided must be a valid enum.", nameof(order));
        if (!nullOrder.IsValid())
            throw new ArgumentException($"The {nameof(IndexColumnNullOrder)} provided must be a valid enum.", nameof(nullOrder));

        Expression = expression;
        DependentColumns = columns;
        Order = order;
        NullOrder = nullOrder;
        Collation = collation;
        PrefixLength = prefixLength;
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
    /// <value>A null ordering, or <see cref="IndexColumnNullOrder.Default"/> when the database
    /// does not report one.</value>
    public IndexColumnNullOrder NullOrder { get; }

    /// <summary>
    /// The collation the index column is sorted by, when the database reports one that is not
    /// simply the database's default collation.
    /// </summary>
    /// <value>A collation name, if the database reports one for the index column.</value>
    public Option<Identifier> Collation { get; }

    /// <summary>
    /// The number of leading characters or bytes of the column that are indexed, for a prefix index.
    /// </summary>
    /// <value>A prefix length, if the index covers only a prefix of the column's values.</value>
    public Option<int> PrefixLength { get; }

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