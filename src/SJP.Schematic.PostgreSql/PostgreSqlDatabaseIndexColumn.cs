using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.PostgreSql;

// TODO: remove this when the dependent columns can be parsed out

/// <summary>
/// A MySQL definition of an index column.
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
    /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <c>null</c>, empty or whitespace.</exception>
    /// <exception cref="ArgumentException"><paramref name="order"/> is an invalid enum value.</exception>
    public PostgreSqlDatabaseIndexColumn(string expression, IndexColumnOrder order)
    {
        if (expression.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(expression));
        if (!order.IsValid())
            throw new ArgumentException($"The {nameof(IndexColumnOrder)} provided must be a valid enum.", nameof(order));

        Expression = expression;
        Order = order;
        DependentColumns = Array.Empty<IDatabaseColumn>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDatabaseIndexColumn"/> class.
    /// </summary>
    /// <param name="expression">An expression that represents the index column.</param>
    /// <param name="column">A database column the index is dependent on.</param>
    /// <param name="order">The sorting order applied to the index column.</param>
    /// <exception cref="ArgumentNullException"><paramref name="column"/> is <c>null</c>. Alternatively if <paramref name="expression"/> is <c>null</c>, empty or whitespace.</exception>
    /// <exception cref="ArgumentException"><paramref name="order"/> is an invalid enum value.</exception>
    public PostgreSqlDatabaseIndexColumn(string expression, IDatabaseColumn column, IndexColumnOrder order)
    {
        if (expression.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(expression));
        ArgumentNullException.ThrowIfNull(column);
        if (!order.IsValid())
            throw new ArgumentException($"The {nameof(IndexColumnOrder)} provided must be a valid enum.", nameof(order));

        Expression = expression;
        DependentColumns = new[] { column };
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