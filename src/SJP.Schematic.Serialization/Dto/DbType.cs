using System.Collections.Generic;

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
    /// against the assemblies that are already loaded. A name that cannot be resolved is still kept
    /// as the type's name, so reading and writing a document does not discard it.
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

    /// <summary>
    /// The type of the elements stored by a collection type, if any.
    /// </summary>
    public DbType? ElementType { get; init; }

    /// <summary>
    /// The values a value of this type is restricted to, if any.
    /// </summary>
    public IEnumerable<string>? EnumValues { get; init; }

    /// <summary>
    /// The type that this type is defined in terms of, if any.
    /// </summary>
    public DbType? BaseType { get; init; }

    /// <summary>
    /// Whether the data type stores only non-negative values.
    /// </summary>
    public bool IsUnsigned { get; init; }
}
