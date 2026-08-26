namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized column data type.
/// </summary>
public sealed record DbType
{
    /// <summary>
    /// The name of the data type.
    /// </summary>
    public required Identifier TypeName { get; init; }

    /// <summary>
    /// The class of data type.
    /// </summary>
    public required Core.DataType DataType { get; init; }

    /// <summary>
    /// The definition of the data type, as declared by the database.
    /// </summary>
    public required string Definition { get; init; }

    /// <summary>
    /// Whether the data type has a fixed length.
    /// </summary>
    public required bool IsFixedLength { get; init; }

    /// <summary>
    /// The maximum length a value of this type can hold.
    /// </summary>
    public required int MaxLength { get; init; }

    /// <summary>
    /// The name of the CLR type used to store column data, if available.
    /// </summary>
    /// <remarks>
    /// The name is written without any assembly information, and is resolved on deserialization
    /// against the assemblies that are already loaded.
    /// </remarks>
    public string? ClrTypeName { get; init; }

    /// <summary>
    /// The numeric precision of the data type, if any.
    /// </summary>
    public NumericPrecision? NumericPrecision { get; init; }

    /// <summary>
    /// The collation applied to the data type, if any.
    /// </summary>
    public Identifier? Collation { get; init; }
}
