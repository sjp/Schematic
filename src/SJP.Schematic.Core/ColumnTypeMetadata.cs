using System;
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
    /// Gets or sets the type of the color.
    /// </summary>
    /// <value>
    /// The type of the color.
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
}