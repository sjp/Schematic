using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized column within a database index.
/// </summary>
public class DatabaseIndexColumn
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
}
