using System;

namespace SJP.Schematic.Core;

/// <summary>
/// Generic data type classes.
/// </summary>
public enum DataType
{
    /// <summary>
    /// An unknown data type, usually indicative of missing behaviour or a bug.
    /// </summary>
    Unknown, // error case

    /// <summary>
    /// A big integer type, usually at least holding 64-bits of data.
    /// </summary>
    BigInteger,

    /// <summary>
    /// A binary data type.
    /// </summary>
    Binary,

    /// <summary>
    /// A boolean data type, intended to store only <c>true</c> or <c>false</c> data.
    /// </summary>
    Boolean,

    /// <summary>
    /// A data type that holds only date information, with no associated time data.
    /// </summary>
    Date,

    /// <summary>
    /// A data type that holds both date and time information.
    /// </summary>
    DateTime,

    /// <summary>
    /// Type representing floating point types, such as <c>FLOAT</c> or <c>REAL</c>.
    /// </summary>
    Float,

    /// <summary>
    /// A type which stores common integers, typically 32-bit integers.
    /// </summary>
    Integer,

    /// <summary>
    /// A type which stores the duration of a time period.
    /// </summary>
    Interval,

    /// <summary>
    /// A large binary data type. Corresponds to a large and/or un-lengthed binary type for the target platform.
    /// </summary>
    LargeBinary,

    /// <summary>
    /// Type representing floating point types, such as <c>FLOAT</c> or <c>REAL</c>.
    /// </summary>
    Numeric,

    /// <summary>
    /// Stores small integers.
    /// </summary>
    SmallInteger,

    /// <summary>
    /// A variable-length string data type.
    /// </summary>
    String,

    /// <summary>
    /// An unbounded length string type.
    /// </summary>
    Text,

    /// <summary>
    /// A data type that holds only time information, with no associated date data.
    /// </summary>
    Time,

    /// <summary>
    /// A variable length unicode string type.
    /// </summary>
    Unicode,

    /// <summary>
    /// An unbounded length unicode string type.
    /// </summary>
    UnicodeText,

    /// <summary>
    /// Stores data that are unique identifiers, e.g. <see cref="Guid"/> values.
    /// </summary>
    UniqueIdentifier
}