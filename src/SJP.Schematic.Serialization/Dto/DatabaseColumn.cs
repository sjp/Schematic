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
/// identical.
/// </para>
/// <para>
/// The copies do not multiply objects in memory. When a whole database is mapped to its serialized
/// form, an object referenced from several places is mapped to a single shared instance. When it is
/// mapped back, a copy held by a key, an index or a foreign key is read as the column of the same name
/// in the table or view that owns it, so a column is one instance wherever it is referenced, as it is
/// when read from a live database. Names are matched exactly, and the rest of the copy is not read, so
/// the owning object's definition of the column is the one that counts. A copy is read as a column of
/// its own when no such column exists: when it names a column its table does not list, or belongs to a
/// table missing from the document, named more than once in it, or not mapped along with it because
/// a single table was mapped on its own.
/// </para>
/// </remarks>
public sealed record DatabaseColumn
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
    /// Whether the column is omitted from the expansion of <c>SELECT *</c>.
    /// </summary>
    /// <remarks>
    /// Not required, so that a document written before hidden columns were described still reads
    /// back, as a visible column.
    /// </remarks>
    public bool IsHidden { get; init; }

    /// <summary>
    /// The expression applied when no value is provided for the column, if any.
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// The name of the constraint carrying the default, where the source database models a default
    /// as a constraint in its own right. Only SQL Server does.
    /// </summary>
    public Identifier? DefaultConstraintName { get; init; }

    /// <summary>
    /// What <see cref="DefaultValue"/> evaluates to.
    /// </summary>
    /// <remarks>
    /// Not required, so that a document written before defaults were classified still reads back, as
    /// an unknown kind.
    /// </remarks>
    public Core.DefaultValueKind DefaultValueKind { get; init; }

    /// <summary>
    /// The sequence a default draws its values from, when <see cref="DefaultValueKind"/> is
    /// <see cref="Core.DefaultValueKind.SequenceNextValue"/> and the source database named one.
    /// </summary>
    public Identifier? DefaultSequenceName { get; init; }

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

    /// <summary>
    /// Whether a computed column's values are stored with the row or evaluated when the column is read.
    /// </summary>
    /// <remarks>
    /// Not required, so that a document written before computed columns carried their storage still
    /// reads back, as an unknown storage.
    /// </remarks>
    public Core.ComputedColumnStorage ComputedStorage { get; init; }
}
