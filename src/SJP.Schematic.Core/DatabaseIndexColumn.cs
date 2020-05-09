using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
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
        /// <exception cref="ArgumentNullException"><paramref name="column"/> is <c>null</c>. Alternatively if <paramref name="expression"/> is <c>null</c>, empty or whitespace.</exception>
        /// <exception cref="ArgumentException"><paramref name="order"/> is an invalid enum.</exception>
        public DatabaseIndexColumn(string expression, IDatabaseColumn column, IndexColumnOrder order)
            : this(expression, new[] { column }, order)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseIndexColumn"/> class.
        /// </summary>
        /// <param name="expression">A textual expression defining the index column.</param>
        /// <param name="dependentColumns">Columns that the index column is dependent upon.</param>
        /// <param name="order">The index column ordering.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dependentColumns"/> is <c>null</c> or contains <c>null</c> values. Alternatively if <paramref name="expression"/> is <c>null</c>, empty or whitespace.</exception>
        /// <exception cref="ArgumentException"><paramref name="order"/> is an invalid enum.</exception>
        public DatabaseIndexColumn(string expression, IEnumerable<IDatabaseColumn> dependentColumns, IndexColumnOrder order)
        {
            if (expression.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(expression));
            if (dependentColumns == null || dependentColumns.AnyNull())
                throw new ArgumentNullException(nameof(dependentColumns));
            if (!order.IsValid())
                throw new ArgumentException($"The { nameof(IndexColumnOrder) } provided must be a valid enum.", nameof(order));

            Expression = expression;
            DependentColumns = dependentColumns.ToList();
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
}
