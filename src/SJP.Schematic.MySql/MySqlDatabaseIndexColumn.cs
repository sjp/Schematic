using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
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
    /// <exception cref="ArgumentNullException"><paramref name="column"/> is <c>null</c>. Alternatively if <paramref name="expression"/> is <c>null</c>, empty or whitespace.</exception>
    public MySqlDatabaseIndexColumn(string expression, IDatabaseColumn column)
    {
        ArgumentNullException.ThrowIfNull(column);
        if (expression.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(expression));

        Expression = expression;
        DependentColumns = new[] { column };
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