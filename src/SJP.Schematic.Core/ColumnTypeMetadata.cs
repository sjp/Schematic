using System;
using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// A simple container type that stores database column type metadata.
/// </summary>
public class ColumnTypeMetadata
{
    /// <summary>
    /// The type name of the column data type.
    /// </summary>
    /// <value>The name of the type.</value>
    public Identifier? TypeName { get; set; }

    /// <summary>
    /// The generic type of the data type.
    /// </summary>
    /// <value>A generic class of the data type.</value>
    public DataType DataType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column type has a fixed length.
    /// </summary>
    /// <value><see langword="true" /> if this type has fixed length; otherwise, <see langword="false" />.</value>
    public bool IsFixedLength { get; set; }

    /// <summary>
    /// The maximum length of the column type.
    /// </summary>
    /// <value>The maximum length.</value>
    public int MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the CLR type that the column data type maps to.
    /// </summary>
    /// <value>
    /// The CLR type.
    /// </value>
    public Type? ClrType { get; set; }

    /// <summary>
    /// If available, the collation applies to the column's type.
    /// </summary>
    /// <value>The collation.</value>
    public Option<Identifier> Collation { get; set; }

    /// <summary>
    /// A numeric precision, if available, that describes the size of the numeric value storage.
    /// </summary>
    /// <value>The numeric precision.</value>
    public Option<INumericPrecision> NumericPrecision { get; set; }

    /// <summary>
    /// The number of digits kept after the decimal point in the seconds of a temporal value, if available.
    /// </summary>
    /// <value>The fractional seconds precision, for a time, timestamp or interval type that declares one; otherwise none.</value>
    public Option<int> FractionalSecondsPrecision { get; set; }

    /// <summary>
    /// The type of the elements stored by a collection type, if available.
    /// </summary>
    /// <value>The element type, for a <see cref="Core.DataType.Array"/> or <see cref="Core.DataType.Range"/> type; otherwise none.</value>
    public Option<IDbType> ElementType { get; set; }

    /// <summary>
    /// The values a value of this type is restricted to.
    /// </summary>
    /// <value>The permitted values, for a <see cref="Core.DataType.Enum"/> or <see cref="Core.DataType.Set"/> type; otherwise empty.</value>
    public IReadOnlyList<string> EnumValues { get; set; } = [];

    /// <summary>
    /// The type that this type is defined in terms of, if available.
    /// </summary>
    /// <value>The base type, for a domain or alias type; otherwise none.</value>
    public Option<IDbType> BaseType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the type stores only non-negative values.
    /// </summary>
    /// <value><see langword="true" /> if the type is unsigned; otherwise, <see langword="false" />.</value>
    public bool IsUnsigned { get; set; }
}