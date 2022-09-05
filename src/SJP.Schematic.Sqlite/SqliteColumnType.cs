using System;
using System.Collections.Generic;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
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
    {
        if (!typeAffinity.IsValid())
            throw new ArgumentException($"The {nameof(SqliteTypeAffinity)} provided must be a valid enum.", nameof(typeAffinity));

        var typeName = typeAffinity.ToString().ToUpperInvariant();
        TypeName = typeName;
        Definition = typeName;

        DataType = _affinityTypeMap[typeAffinity];
        ClrType = _affinityClrTypeMap[typeAffinity];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteColumnType"/> class.
    /// </summary>
    /// <param name="typeAffinity">The type affinity.</param>
    /// <param name="collation">The collation.</param>
    /// <exception cref="ArgumentException"><paramref name="collation"/> or <paramref name="typeAffinity"/> are invalid enum values. Alternatively if the <paramref name="collation"/> is not <see cref="SqliteTypeAffinity.Text"/>.</exception>
    public SqliteColumnType(SqliteTypeAffinity typeAffinity, SqliteCollation collation)
        : this(typeAffinity)
    {
        if (!typeAffinity.IsValid())
            throw new ArgumentException($"The {nameof(SqliteTypeAffinity)} provided must be a valid enum.", nameof(typeAffinity));
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
    /// <value><c>true</c> if this instance has a fixed length; otherwise, <c>false</c>.</value>
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
    /// The numeric precision, if available.
    /// </summary>
    /// <value>The numeric precision. Always unavailable.</value>
    public Option<INumericPrecision> NumericPrecision { get; } = Option<INumericPrecision>.None;

    /// <summary>
    /// The collation, if available.
    /// </summary>
    /// <value>The collation.</value>
    public Option<Identifier> Collation { get; }

    private readonly IReadOnlyDictionary<SqliteTypeAffinity, DataType> _affinityTypeMap = new Dictionary<SqliteTypeAffinity, DataType>
    {
        [SqliteTypeAffinity.Blob] = DataType.LargeBinary,
        [SqliteTypeAffinity.Integer] = DataType.BigInteger,
        [SqliteTypeAffinity.Numeric] = DataType.Numeric,
        [SqliteTypeAffinity.Real] = DataType.Float,
        [SqliteTypeAffinity.Text] = DataType.UnicodeText
    };

    private readonly IReadOnlyDictionary<SqliteTypeAffinity, Type> _affinityClrTypeMap = new Dictionary<SqliteTypeAffinity, Type>
    {
        [SqliteTypeAffinity.Blob] = typeof(byte[]),
        [SqliteTypeAffinity.Integer] = typeof(long),
        [SqliteTypeAffinity.Numeric] = typeof(decimal),
        [SqliteTypeAffinity.Real] = typeof(double),
        [SqliteTypeAffinity.Text] = typeof(string)
    };
}