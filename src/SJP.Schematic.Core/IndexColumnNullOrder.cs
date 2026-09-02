namespace SJP.Schematic.Core;

/// <summary>
/// Describes where null values sort relative to non-null values within an index column.
/// </summary>
public enum IndexColumnNullOrder
{
    /// <summary>
    /// The null ordering is not reported by the database, so its default for the column's
    /// sort direction applies.
    /// </summary>
    Default,

    /// <summary>
    /// Null values sort before non-null values.
    /// </summary>
    NullsFirst,

    /// <summary>
    /// Null values sort after non-null values.
    /// </summary>
    NullsLast,
}
