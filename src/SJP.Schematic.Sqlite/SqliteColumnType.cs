using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Parsing;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// Defines a column type specific to SQLite.
/// </summary>
/// <seealso cref="IDbType" />
public class SqliteColumnType : IDbType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteColumnType"/> class.
    /// </summary>
    /// <param name="typeAffinity">The type affinity.</param>
    /// <exception cref="ArgumentException"><paramref name="typeAffinity"/> is an invalid enum value.</exception>
    public SqliteColumnType(SqliteTypeAffinity typeAffinity)
        : this(null, typeAffinity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteColumnType"/> class.
    /// </summary>
    /// <param name="typeAffinity">The type affinity.</param>
    /// <param name="collation">The collation.</param>
    /// <exception cref="ArgumentException"><paramref name="collation"/> or <paramref name="typeAffinity"/> are invalid enum values. Alternatively if the <paramref name="collation"/> is not <see cref="SqliteTypeAffinity.Text"/>.</exception>
    public SqliteColumnType(SqliteTypeAffinity typeAffinity, SqliteCollation collation)
        : this(null, typeAffinity, collation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteColumnType"/> class, preserving the type as it was declared.
    /// </summary>
    /// <param name="declaredTypeName">The type as it was declared in the table definition, e.g. <c>VARCHAR(50)</c>. When absent, the affinity names the type instead.</param>
    /// <param name="typeAffinity">The type affinity.</param>
    /// <exception cref="ArgumentException"><paramref name="typeAffinity"/> is an invalid enum value.</exception>
    /// <remarks>
    /// SQLite stores values by affinity rather than by declared type, so the affinity determines
    /// <see cref="DataType"/> and <see cref="ClrType"/>. The declared type is nevertheless what the
    /// table definition says, so it is reported unchanged as the type name and definition.
    /// </remarks>
    public SqliteColumnType(string? declaredTypeName, SqliteTypeAffinity typeAffinity)
    {
        if (!typeAffinity.IsValid())
            throw new ArgumentException($"The {nameof(SqliteTypeAffinity)} provided must be a valid enum.", nameof(typeAffinity));

        var affinityName = typeAffinity.ToString().ToUpperInvariant();
        var definition = declaredTypeName?.Trim();
        Definition = definition.IsNullOrWhiteSpace() ? affinityName : definition;
        TypeName = GetTypeNameWithoutArguments(Definition);

        DataType = AffinityTypeMap[typeAffinity];
        ClrType = AffinityClrTypeMap[typeAffinity];
        ClrTypeName = ClrType.ToString();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteColumnType"/> class, preserving the type as it was declared.
    /// </summary>
    /// <param name="declaredTypeName">The type as it was declared in the table definition, e.g. <c>VARCHAR(50)</c>. When absent, the affinity names the type instead.</param>
    /// <param name="typeAffinity">The type affinity.</param>
    /// <param name="collation">The collation.</param>
    /// <exception cref="ArgumentException"><paramref name="collation"/> or <paramref name="typeAffinity"/> are invalid enum values. Alternatively if the <paramref name="collation"/> is not <see cref="SqliteTypeAffinity.Text"/>.</exception>
    public SqliteColumnType(string? declaredTypeName, SqliteTypeAffinity typeAffinity, SqliteCollation collation)
        : this(declaredTypeName, typeAffinity)
    {
        if (!collation.IsValid())
            throw new ArgumentException($"The {nameof(SqliteCollation)} provided must be a valid enum.", nameof(collation));
        if (typeAffinity != SqliteTypeAffinity.Text)
            throw new ArgumentException("The type affinity must be a text type when a collation has been provided.", nameof(typeAffinity));

        Collation = collation != SqliteCollation.None
            ? Option<Identifier>.Some(collation.ToString().ToUpperInvariant())
            : Option<Identifier>.None;
    }

    /// <summary>
    /// Gets the name of the column data type.
    /// </summary>
    /// <value>The name of the type.</value>
    public Identifier TypeName { get; }

    /// <summary>
    /// Gets the class of data type.
    /// </summary>
    /// <value>The data type.</value>
    public DataType DataType { get; }

    /// <summary>
    /// Gets the definition.
    /// </summary>
    /// <value>The definition.</value>
    public string Definition { get; }

    /// <summary>
    /// Gets a value indicating whether this data type has fixed length.
    /// </summary>
    /// <value><see langword="true" /> if this instance has a fixed length; otherwise, <see langword="false" />.</value>
    public bool IsFixedLength { get; }

    /// <summary>
    /// The maximum length the column can hold.
    /// </summary>
    /// <value>The maximum length. Always -1 (i.e. unknown).</value>
    public int MaxLength { get; } = -1;

    /// <summary>
    /// The CLR data type used to store column data.
    /// </summary>
    /// <value>A CLR type.</value>
    public Type ClrType { get; }

    /// <summary>
    /// The name of the CLR data type used to store column data.
    /// </summary>
    /// <value>The name of <see cref="ClrType"/>, without any assembly information.</value>
    public string ClrTypeName { get; }

    /// <summary>
    /// The numeric precision, if available.
    /// </summary>
    /// <value>The numeric precision. Always unavailable.</value>
    public Option<INumericPrecision> NumericPrecision { get; } = Option<INumericPrecision>.None;

    /// <summary>
    /// The collation, if available.
    /// </summary>
    /// <value>The collation.</value>
    public Option<Identifier> Collation { get; }

    /// <summary>
    /// The number of digits kept after the decimal point in the seconds of a temporal value, if available.
    /// </summary>
    /// <value>Always unavailable; SQLite has no temporal type, so nothing declares a fractional seconds precision.</value>
    public Option<int> FractionalSecondsPrecision { get; } = Option<int>.None;

    /// <summary>
    /// The type of the elements stored by a collection type, if available.
    /// </summary>
    /// <value>The element type. Always unavailable; SQLite has no collection types.</value>
    public Option<IDbType> ElementType { get; } = Option<IDbType>.None;

    /// <summary>
    /// The values a value of this type is restricted to.
    /// </summary>
    /// <value>The permitted values. Always empty; SQLite has no enumerated types.</value>
    public IReadOnlyList<string> EnumValues { get; } = [];

    /// <summary>
    /// The type that this type is defined in terms of, if available.
    /// </summary>
    /// <value>The base type. Always unavailable; SQLite has no domain or alias types.</value>
    public Option<IDbType> BaseType { get; } = Option<IDbType>.None;

    /// <summary>
    /// Gets a value indicating whether the type stores only non-negative values.
    /// </summary>
    /// <value>Always <see langword="false" />; SQLite has no unsigned types.</value>
    public bool IsUnsigned { get; }

    // a declared type may carry arguments, e.g. VARCHAR(50) or DECIMAL(10, 2), which are part of
    // the definition but not of the name
    private static string GetTypeNameWithoutArguments(string definition)
    {
        var parenIndex = definition.IndexOf('(', StringComparison.Ordinal);
        return parenIndex < 0
            ? definition
            : definition[..parenIndex].TrimEnd();
    }

    private static readonly FrozenDictionary<SqliteTypeAffinity, DataType> AffinityTypeMap = new Dictionary<SqliteTypeAffinity, DataType>
    {
        [SqliteTypeAffinity.Blob] = DataType.LargeBinary,
        [SqliteTypeAffinity.Integer] = DataType.BigInteger,
        [SqliteTypeAffinity.Numeric] = DataType.Numeric,
        [SqliteTypeAffinity.Real] = DataType.Float,
        [SqliteTypeAffinity.Text] = DataType.UnicodeText,
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<SqliteTypeAffinity, Type> AffinityClrTypeMap = new Dictionary<SqliteTypeAffinity, Type>
    {
        [SqliteTypeAffinity.Blob] = typeof(byte[]),
        [SqliteTypeAffinity.Integer] = typeof(long),
        [SqliteTypeAffinity.Numeric] = typeof(decimal),
        [SqliteTypeAffinity.Real] = typeof(double),
        [SqliteTypeAffinity.Text] = typeof(string),
    }.ToFrozenDictionary();
}
