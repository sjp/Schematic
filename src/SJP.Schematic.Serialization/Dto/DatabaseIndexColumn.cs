using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized column within a database index.
/// </summary>
public sealed record DatabaseIndexColumn
{
    /// <summary>
    /// The ordering applied to the column.
    /// </summary>
    public required Core.IndexColumnOrder Order { get; init; }

    /// <summary>
    /// The set of columns that the index column is dependent upon.
    /// </summary>
    /// <remarks>
    /// Each column is written out in full rather than referenced by name; see <see cref="DatabaseColumn"/>
    /// for why.
    /// </remarks>
    public required IEnumerable<DatabaseColumn> DependentColumns { get; init; }

    /// <summary>
    /// The expression that represents the index column, e.g. <c>UPPER(name)</c>.
    /// </summary>
    public required string Expression { get; init; }

    /// <summary>
    /// Where null values sort relative to non-null values within the column.
    /// </summary>
    /// <remarks>
    /// Not required, so that a document written before index columns carried a null ordering still
    /// reads back, as the database's default ordering.
    /// </remarks>
    public Core.IndexColumnNullOrder NullOrder { get; init; }

    /// <summary>
    /// The collation the index column is sorted by, if the source database reported one.
    /// </summary>
    public Identifier? Collation { get; init; }

    /// <summary>
    /// The number of leading characters or bytes indexed, for a prefix index.
    /// </summary>
    public int? PrefixLength { get; init; }
}
