using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Defines the
    /// </summary>
    public interface IDatabaseIndexColumn
    {
        /// <summary>
        /// The ordering applied to the column.
        /// </summary>
        /// <value>The ordering.</value>
        IndexColumnOrder Order { get; }

        /// <summary>
        /// An expression that represents the given index column e.g. <c>UPPER(name)</c>.
        /// </summary>
        /// <value>A textual expression.</value>
        string Expression { get; }

        /// <summary>
        /// The set of columns that the index column is dependent upon.
        /// </summary>
        /// <value>The dependent columns.</value>
        IReadOnlyList<IDatabaseColumn> DependentColumns { get; }
    }
}
