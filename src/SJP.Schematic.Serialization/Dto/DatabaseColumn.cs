namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized database column.
/// </summary>
/// <remarks>
/// <para>
/// Columns are serialized by value everywhere they appear. A column that participates in a key or an
/// index is written out again in full alongside the copy held in its table's column list, instead of
/// being referenced by name.
/// </para>
/// <para>
/// This is deliberate. Every serialized object stays self-contained and can be read without a
/// resolution pass over the rest of the document: foreign keys in particular describe columns of
/// another table, which may appear later in the document or — when a partial database is serialized —
/// not at all, so name references could not always be resolved. It also keeps the per-type mappers
/// independent of one another, so a single table or key can be mapped on its own.
/// </para>
/// <para>
/// The cost is redundancy. On a sixteen-table schema the repeated columns account for roughly a third
/// of an uncompressed export, though far less once compressed, as the repeated fragments are near
/// identical. The other consequence is that deserialization produces a distinct instance per copy: a
/// table's column and the same column reached through its primary key or an index match by name, never
/// by reference. Nothing in Schematic compares columns by instance, so this is a difference from live
/// providers rather than a defect, but a consumer that relies on reference identity will see it.
/// </para>
/// </remarks>
public class DatabaseColumn
{
    /// <summary>
    /// The name of the column.
    /// </summary>
    public required Identifier ColumnName { get; init; }

    /// <summary>
    /// Whether the column accepts <c>NULL</c> values.
    /// </summary>
    public required bool IsNullable { get; init; }

    /// <summary>
    /// Whether the column's value is computed from an expression rather than stored.
    /// </summary>
    public bool IsComputed { get; init; }

    /// <summary>
    /// The expression applied when no value is provided for the column, if any.
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// The type of data the column stores.
    /// </summary>
    public required DbType Type { get; init; }

    /// <summary>
    /// The auto-incrementing sequence attached to the column, if any.
    /// </summary>
    public AutoIncrement? AutoIncrement { get; init; }

    /// <summary>
    /// The expression a computed column is defined by, if any.
    /// </summary>
    public string? Definition { get; init; }
}
