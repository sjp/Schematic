using System;
using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines column data type information.
/// </summary>
public interface IDbType
{
    /// <summary>
    /// Gets the name of the column data type.
    /// </summary>
    /// <value>The name of the type.</value>
    Identifier TypeName { get; }

    /// <summary>
    /// Gets the class of data type.
    /// </summary>
    /// <value>The data type.</value>
    DataType DataType { get; }

    /// <summary>
    /// Gets the definition.
    /// </summary>
    /// <value>The definition.</value>
    string Definition { get; }

    /// <summary>
    /// Gets a value indicating whether this data type has fixed length.
    /// </summary>
    /// <value><see langword="true" /> if this instance has a fixed length; otherwise, <see langword="false" />.</value>
    bool IsFixedLength { get; }

    /// <summary>
    /// The maximum length the column can hold.
    /// </summary>
    /// <value>The maximum length.</value>
    int MaxLength { get; }

    /// <summary>
    /// The CLR data type used to store column data.
    /// </summary>
    /// <value>A CLR type. <see cref="object"/> when the type named by <see cref="ClrTypeName"/> could not be resolved.</value>
    Type ClrType { get; }

    /// <summary>
    /// The name of the CLR data type used to store column data.
    /// </summary>
    /// <value>A type name without any assembly information, e.g. <c>System.String</c>.</value>
    /// <remarks>
    /// This is the type as it was named by whatever described the column, so it survives being
    /// written to and read back from a document, even in a process where the assembly declaring the
    /// type is not loaded and <see cref="ClrType"/> is therefore <see cref="object"/>.
    /// </remarks>
    string ClrTypeName { get; }

    /// <summary>
    /// The numeric precision, if available.
    /// </summary>
    /// <value>The numeric precision.</value>
    Option<INumericPrecision> NumericPrecision { get; }

    /// <summary>
    /// The collation, if available.
    /// </summary>
    /// <value>The collation.</value>
    Option<Identifier> Collation { get; }

    /// <summary>
    /// The type of the elements stored by a collection type, if available.
    /// </summary>
    /// <value>The element type, for a <see cref="DataType.Array"/> or <see cref="DataType.Range"/> type; otherwise none.</value>
    Option<IDbType> ElementType { get; }

    /// <summary>
    /// The values a value of this type is restricted to.
    /// </summary>
    /// <value>The permitted values, for a <see cref="DataType.Enum"/> or <see cref="DataType.Set"/> type; otherwise empty.</value>
    IReadOnlyList<string> EnumValues { get; }

    /// <summary>
    /// The type that this type is defined in terms of, if available.
    /// </summary>
    /// <value>The base type, for a domain or alias type; otherwise none.</value>
    Option<IDbType> BaseType { get; }

    /// <summary>
    /// Gets a value indicating whether the type stores only non-negative values.
    /// </summary>
    /// <value><see langword="true" /> if this instance is unsigned; otherwise, <see langword="false" />.</value>
    bool IsUnsigned { get; }
}