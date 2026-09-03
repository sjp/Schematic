namespace SJP.Schematic.Core;

/// <summary>
/// <para>Describes how a foreign key with more than one column treats child rows that are partially <c>null</c>.</para>
/// <para>Databases that support only one behaviour report <see cref="Simple"/>.</para>
/// </summary>
public enum ForeignKeyMatchType
{
    /// <summary>
    /// The constraint is satisfied when any of the child key's columns is <c>null</c>. This is the
    /// SQL default, and the only behaviour most database engines implement.
    /// </summary>
    Simple,

    /// <summary>
    /// The constraint requires the non-<c>null</c> child columns to match a parent row, while
    /// allowing the remaining columns to be <c>null</c>.
    /// </summary>
    Partial,

    /// <summary>
    /// The constraint requires either all of the child key's columns to be <c>null</c>, or all of
    /// them to match a parent row.
    /// </summary>
    Full,
}
