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
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class DatabaseIndexColumn : IDatabaseIndexColumn
    {
        public DatabaseIndexColumn(string expression, IDatabaseColumn column, IndexColumnOrder order)
            : this(expression, new[] { column }, order)
        {
        }

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

        public string Expression { get; }

        public IReadOnlyList<IDatabaseColumn> DependentColumns { get; }

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
