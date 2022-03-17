using System;
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
    /// <value><c>true</c> if this instance has a fixed length; otherwise, <c>false</c>.</value>
    bool IsFixedLength { get; }

    /// <summary>
    /// The maximum length the column can hold.
    /// </summary>
    /// <value>The maximum length.</value>
    int MaxLength { get; }

    /// <summary>
    /// The CLR data type used to store column data.
    /// </summary>
    /// <value>A CLR type.</value>
    Type ClrType { get; }

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
}
