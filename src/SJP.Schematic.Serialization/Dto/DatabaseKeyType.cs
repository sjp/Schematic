namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// The kind of a serialized key constraint, recording which case of <see cref="Core.DatabaseKey"/> it was written from.
/// </summary>
public enum DatabaseKeyType
{
    /// <summary>
    /// Primary key.
    /// </summary>
    Primary,

    /// <summary>
    /// Unique key.
    /// </summary>
    Unique,

    /// <summary>
    /// Foreign key.
    /// </summary>
    Foreign,
}