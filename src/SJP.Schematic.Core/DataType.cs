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
    /// A boolean data type, intended to store only <see langword="true" /> or <see langword="false" /> data.
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
    /// A spatial data type which stores geometric or geographic values.
    /// </summary>
    Geometry,

    /// <summary>
    /// A type which stores common integers, typically 32-bit integers.
    /// </summary>
    Integer,

    /// <summary>
    /// A type which stores the duration of a time period.
    /// </summary>
    Interval,

    /// <summary>
    /// A type which stores JSON (JavaScript Object Notation) documents.
    /// </summary>
    Json,

    /// <summary>
    /// A large binary data type. Corresponds to a large and/or un-lengthed binary type for the target platform.
    /// </summary>
    LargeBinary,

    /// <summary>
    /// Type representing exact fixed-point types, such as <c>NUMERIC</c> or <c>DECIMAL</c>.
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
    UniqueIdentifier,

    /// <summary>
    /// A type which stores XML (eXtensible Markup Language) documents.
    /// </summary>
    Xml,

    /// <summary>
    /// Stores very small integers, typically a single byte.
    /// </summary>
    TinyInteger,

    /// <summary>
    /// A data type that holds date and time information together with an offset from UTC.
    /// </summary>
    DateTimeOffset,

    /// <summary>
    /// A data type that holds time information together with an offset from UTC, with no associated date data.
    /// </summary>
    TimeOffset,

    /// <summary>
    /// A fixed-point type dedicated to storing currency amounts.
    /// </summary>
    Money,

    /// <summary>
    /// A string of bits, addressed bit by bit rather than byte by byte.
    /// </summary>
    Bit,

    /// <summary>
    /// An opaque value that the database replaces whenever the row changes, used for optimistic concurrency.
    /// </summary>
    RowVersion,

    /// <summary>
    /// A collection of values of a single element type. See <see cref="IDbType.ElementType"/> for the element type.
    /// </summary>
    Array,

    /// <summary>
    /// A type restricted to a fixed set of named values. See <see cref="IDbType.EnumValues"/> for the permitted values.
    /// </summary>
    Enum,

    /// <summary>
    /// A type storing any subset of a fixed set of named values. See <see cref="IDbType.EnumValues"/> for the permitted values.
    /// </summary>
    Set,

    /// <summary>
    /// A type storing a range or multirange of values of a single element type. See <see cref="IDbType.ElementType"/> for the element type.
    /// </summary>
    Range,

    /// <summary>
    /// A type storing a network address, such as an IP address, a subnet or a MAC address.
    /// </summary>
    Network,

    /// <summary>
    /// A structured type built out of named attributes, such as a composite, object or table type.
    /// </summary>
    Composite,

    /// <summary>
    /// A type able to store a value of any other data type, such as <c>SQL_VARIANT</c> or <c>ANYDATA</c>.
    /// </summary>
    Variant,

    /// <summary>
    /// A fixed-dimension vector of numbers, typically used for similarity search over embeddings.
    /// </summary>
    Vector,

    /// <summary>
    /// A type storing a document or query prepared for full-text search.
    /// </summary>
    FullTextSearch,

    /// <summary>
    /// A data type the dialect recognises but which has no more specific class in this enumeration.
    /// Unlike <see cref="Unknown"/>, this is not indicative of a bug.
    /// </summary>
    Other,
}