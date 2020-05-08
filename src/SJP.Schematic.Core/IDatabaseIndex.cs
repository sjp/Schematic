using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Defines a database index.
    /// </summary>
    /// <seealso cref="IDatabaseOptional" />
    public interface IDatabaseIndex : IDatabaseOptional
    {
        /// <summary>
        /// The index name.
        /// </summary>
        /// <value>The name of the index.</value>
        Identifier Name { get; }

        /// <summary>
        /// The index columns that form the primary basis of the index.
        /// </summary>
        /// <value>A collection of index columns.</value>
        IReadOnlyCollection<IDatabaseIndexColumn> Columns { get; }

        /// <summary>
        /// The included or leaf columns that are also available once the key columns have been searched.
        /// </summary>
        /// <value>A collection of database columns.</value>
        IReadOnlyCollection<IDatabaseColumn> IncludedColumns { get; }

        /// <summary>
        /// Indicates whether covered index columns must be unique across the index column set.
        /// </summary>
        /// <value><c>true</c> if the index column set must have unique values; otherwise, <c>false</c>.</value>
        bool IsUnique { get; }
    }
}
