using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// A SQLite definition of an index column.
/// </summary>
/// <seealso cref="IDatabaseIndexColumn" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class SqliteDatabaseIndexColumn : IDatabaseIndexColumn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDatabaseIndexColumn"/> class.
    /// </summary>
    /// <param name="expression">An expression that represents the index column.</param>
    /// <param name="dependentColumns">The columns that the index column is dependent upon, which is empty for an expression that refers to none.</param>
    /// <param name="order">The sorting order applied to the index column.</param>
    /// <param name="collation">The collating sequence used to compare values in the index column, if one was declared.</param>
    /// <exception cref="ArgumentNullException"><paramref name="expression"/> or <paramref name="dependentColumns"/> is <see langword="null" />, or <paramref name="dependentColumns"/> contains a <see langword="null" /> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> is empty or whitespace, or <paramref name="order"/> is an invalid enum value.</exception>
    public SqliteDatabaseIndexColumn(string expression, IEnumerable<IDatabaseColumn> dependentColumns, IndexColumnOrder order, Option<Identifier> collation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        if (dependentColumns.NullOrAnyNull())
            throw new ArgumentNullException(nameof(dependentColumns));
        if (!order.IsValid())
            throw new ArgumentException($"The {nameof(IndexColumnOrder)} provided must be a valid enum.", nameof(order));

        Expression = expression;
        DependentColumns = dependentColumns.ToList();
        Order = order;
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
    /// <value>The dependent columns, which is empty for an expression that refers to no column.</value>
    public IReadOnlyList<IDatabaseColumn> DependentColumns { get; }

    /// <summary>
    /// The ordering applied to the column.
    /// </summary>
    /// <value>The ordering.</value>
    public IndexColumnOrder Order { get; }

    /// <summary>
    /// Where null values sort relative to non-null values within the column.
    /// </summary>
    /// <value>Always <see cref="IndexColumnNullOrder.Default"/>. SQLite always sorts nulls first
    /// in an ascending index and last in a descending one.</value>
    public IndexColumnNullOrder NullOrder { get; } = IndexColumnNullOrder.Default;

    /// <summary>
    /// The collating sequence used to compare values in the index column.
    /// </summary>
    /// <value>A collation name, when the index column declares one that is not the column's own.</value>
    public Option<Identifier> Collation { get; }

    /// <summary>
    /// The number of leading characters or bytes of the column that are indexed, for a prefix index.
    /// </summary>
    /// <value>Always 'none'. SQLite has no prefix indexes.</value>
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
