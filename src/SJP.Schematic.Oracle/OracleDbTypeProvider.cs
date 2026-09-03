using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Oracle;

/// <summary>
/// A database column type provider for Oracle.
/// </summary>
/// <seealso cref="IDbTypeProvider" />
public partial class OracleDbTypeProvider : IDbTypeProvider
{
    /// <summary>
    /// Creates a column data type based on provided metadata.
    /// </summary>
    /// <param name="typeMetadata">Column type metadata.</param>
    /// <returns>A column data type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeMetadata"/> is <see langword="null" />.</exception>
    public IDbType CreateColumnType(ColumnTypeMetadata typeMetadata)
    {
        ArgumentNullException.ThrowIfNull(typeMetadata);

        if (typeMetadata.TypeName == null)
            typeMetadata.TypeName = GetDefaultTypeName(typeMetadata);
        if (typeMetadata.DataType == DataType.Unknown)
        {
            typeMetadata.DataType = GetDataType(typeMetadata.TypeName);
            if (typeMetadata.DataType == DataType.Numeric)
            {
                var numericPrecision = typeMetadata.NumericPrecision.Filter(static np => np.Scale == 0);
                numericPrecision.IfSome(np =>
                {
                    typeMetadata.DataType = np.Precision < 8
                        ? DataType.Integer     // 2^32
                        : DataType.BigInteger; // note: could require storing in a decimal instead of long
                });
                if (typeMetadata.NumericPrecision.IsNone)
                {
                    typeMetadata.DataType = typeMetadata.MaxLength < 8
                        ? DataType.Integer     // 2^32
                        : DataType.BigInteger; // note: could require storing in a decimal instead of long
                }
            }
        }
        if (typeMetadata.ClrType == null)
            typeMetadata.ClrType = GetClrType(typeMetadata.TypeName);
        typeMetadata.IsFixedLength = GetIsFixedLength(typeMetadata.TypeName);

        var definition = GetFormattedTypeName(typeMetadata);
        return new ColumnDataType(
            typeMetadata.TypeName,
            typeMetadata.DataType,
            definition,
            typeMetadata.ClrType,
            typeMetadata.IsFixedLength,
            typeMetadata.MaxLength,
            typeMetadata.NumericPrecision,
            typeMetadata.Collation,
            typeMetadata.ElementType,
            typeMetadata.EnumValues,
            typeMetadata.BaseType,
            typeMetadata.IsUnsigned
        );
    }

    /// <summary>
    /// Gets the data type that most closely matches the provided data type.
    /// </summary>
    /// <param name="otherType">An data type to compare with.</param>
    /// <returns>The closest matching column data type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="otherType"/> is <see langword="null" />.</exception>
    public IDbType GetComparableColumnType(IDbType otherType)
    {
        ArgumentNullException.ThrowIfNull(otherType);

        var typeMetadata = new ColumnTypeMetadata
        {
            ClrType = null, // ignoring so we get the default type provided
            Collation = otherType.Collation,
            DataType = otherType.DataType,
            IsFixedLength = otherType.IsFixedLength,
            MaxLength = otherType.MaxLength,
            NumericPrecision = otherType.NumericPrecision,
            TypeName = null, // ignoring so we get a default name generated
            ElementType = otherType.ElementType,
            EnumValues = otherType.EnumValues,
            BaseType = otherType.BaseType,
            IsUnsigned = otherType.IsUnsigned,
        };

        return CreateColumnType(typeMetadata);
    }

    /// <summary>
    /// Gets the length of the is fixed.
    /// </summary>
    /// <param name="typeName">Name of the type.</param>
    /// <returns><see langword="true" /> if the type has a fixed length, otherwise <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    protected static bool GetIsFixedLength(Identifier typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        return FixedLengthTypes.Contains(NormalizeTypeName(typeName.LocalName));
    }

    /// <summary>
    /// Removes the precision and scale arguments from a type name.
    /// </summary>
    /// <param name="typeName">A type name, e.g. <c>INTERVAL DAY(3) TO SECOND(6)</c>.</param>
    /// <returns>The type name without any arguments, e.g. <c>INTERVAL DAY TO SECOND</c>.</returns>
    /// <remarks>
    /// Oracle reports the fractional seconds precision of a timestamp or interval as part of the
    /// type name rather than in a precision column, so <c>TIMESTAMP(6) WITH TIME ZONE</c> and
    /// <c>TIMESTAMP WITH TIME ZONE</c> both arrive as type names and describe the same type.
    /// </remarks>
    protected static string NormalizeTypeName(string typeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

        return typeName.Contains('(', StringComparison.Ordinal)
            ? TypeNameArgumentRegex().Replace(typeName, string.Empty)
            : typeName;
    }

    [GeneratedRegex(@"\s*\(\s*\d+\s*(?:,\s*\d+\s*)?\)", RegexOptions.ExplicitCapture)]
    private static partial Regex TypeNameArgumentRegex();

    /// <summary>
    /// Gets the default name of the type.
    /// </summary>
    /// <param name="typeMetadata">The type metadata.</param>
    /// <returns>A type name for the given type metadata.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeMetadata"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a type is unknown or failed to be parsed.</exception>
    protected static Identifier GetDefaultTypeName(ColumnTypeMetadata typeMetadata)
    {
        ArgumentNullException.ThrowIfNull(typeMetadata);

        return typeMetadata.DataType switch
        {
            DataType.BigInteger => new Identifier("SYS", "NUMBER"),
            DataType.Binary => new Identifier("SYS", "RAW"),
            DataType.LargeBinary => new Identifier("SYS", "BLOB"),
            DataType.Boolean => new Identifier("SYS", "CHAR"),
            DataType.Date => new Identifier("SYS", "DATE"),
            DataType.DateTime => new Identifier("SYS", "TIMESTAMP WITH LOCAL TIME ZONE"),
            DataType.DateTimeOffset or DataType.TimeOffset => new Identifier("SYS", "TIMESTAMP WITH TIME ZONE"),
            DataType.Float => new Identifier("SYS", "FLOAT"),
            DataType.Geometry => new Identifier("MDSYS", "SDO_GEOMETRY"),
            DataType.Integer or DataType.SmallInteger or DataType.TinyInteger => new Identifier("SYS", "NUMBER"),
            DataType.Interval => new Identifier("SYS", "INTERVAL DAY TO SECOND"),
            DataType.Json => new Identifier("SYS", "JSON"),
            DataType.Money or DataType.Numeric => new Identifier("SYS", "NUMBER"),
            DataType.String => typeMetadata.IsFixedLength
                ? new Identifier("SYS", "CHAR")
                : new Identifier("SYS", "VARCHAR2"),
            DataType.Text => new Identifier("SYS", "CLOB"),
            // Oracle has no time-only type; a time of day is stored as an interval since midnight.
            DataType.Time => new Identifier("SYS", "INTERVAL DAY TO SECOND"),
            DataType.Unicode => typeMetadata.IsFixedLength
                ? new Identifier("SYS", "NCHAR")
                : new Identifier("SYS", "NVARCHAR2"),
            DataType.UnicodeText => new Identifier("SYS", "NCLOB"),
            // Oracle has no native GUID type; UUIDs are conventionally stored as RAW(16).
            DataType.UniqueIdentifier => new Identifier("SYS", "RAW"),
            DataType.Vector => new Identifier("SYS", "VECTOR"),
            DataType.Xml => new Identifier("SYS", "XMLTYPE"),
            // Oracle has no bit-string type, and a row version is an opaque binary value.
            DataType.Bit or DataType.RowVersion => new Identifier("SYS", "RAW"),
            // enumerated, set and network values have no type of their own, and are stored as text.
            DataType.Enum or DataType.Set or DataType.Network or DataType.FullTextSearch => new Identifier("SYS", "VARCHAR2"),
            // ANYDATA is the only type able to hold a value whose shape Oracle cannot describe.
            DataType.Array or DataType.Range or DataType.Composite or DataType.Variant or DataType.Other => new Identifier("SYS", "ANYDATA"),
            DataType.Unknown => throw new ArgumentOutOfRangeException(nameof(typeMetadata), "Unable to determine a type name for an unknown data type."),
            _ => throw new ArgumentOutOfRangeException(nameof(typeMetadata), "Unable to determine a type name for data type: " + typeMetadata.DataType.ToString()),
        };
    }

    /// <summary>
    /// Gets the name of the formatted type.
    /// </summary>
    /// <param name="typeMetadata">The type metadata.</param>
    /// <returns>A string representing a type name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeMetadata"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="typeMetadata"/> does not have a type name.</exception>
    protected static string GetFormattedTypeName(ColumnTypeMetadata typeMetadata)
    {
        ArgumentNullException.ThrowIfNull(typeMetadata);
        if (typeMetadata.TypeName == null)
            throw new ArgumentException("The type name is missing. A formatted type name cannot be generated.", nameof(typeMetadata));

        var builder = StringBuilderCache.Acquire(typeMetadata.TypeName.LocalName.Length * 2);
        var typeName = typeMetadata.TypeName;
        if (string.Equals(typeName.Schema, "SYS", StringComparison.OrdinalIgnoreCase))
        {
            builder.Append(QuoteIdentifier(typeName.LocalName));
        }
        else
        {
            // Inlined rather than delegating to QuoteName(), which acquires its own StringBuilder from
            // the same thread-static cache slot this builder was already taken from — the nested
            // Acquire()/Release() pair would otherwise silently drop one of the two builders from the pool.
            if (typeName.Server != null)
                builder.Append(QuoteIdentifier(typeName.Server)).Append('.');
            if (typeName.Database != null)
                builder.Append(QuoteIdentifier(typeName.Database)).Append('.');
            if (typeName.Schema != null)
                builder.Append(QuoteIdentifier(typeName.Schema)).Append('.');
            builder.Append(QuoteIdentifier(typeName.LocalName));
        }

        if (TypeNamesWithNoLengthAnnotation.Contains(NormalizeTypeName(typeName.LocalName)))
            return builder.GetStringAndRelease();

        var npWithPrecisionOrScale = typeMetadata.NumericPrecision.Filter(static np => np.Precision > 0 || np.Scale > 0);
        if (npWithPrecisionOrScale.IsSome)
        {
            npWithPrecisionOrScale.IfSome(precision =>
            {
                builder.Append('(');
                builder.Append(precision.Precision.ToString(CultureInfo.InvariantCulture));
                if (precision.Scale > 0)
                {
                    builder.Append(", ");
                    builder.Append(precision.Scale.ToString(CultureInfo.InvariantCulture));
                }
                builder.Append(')');
            });
        }
        else if (typeMetadata.MaxLength > 0)
        {
            builder.Append('(');
            var maxLength = typeMetadata.MaxLength;
            builder.Append(maxLength.ToString(CultureInfo.InvariantCulture));
            builder.Append(')');
        }

        return builder.GetStringAndRelease();
    }

    /// <summary>
    /// Gets the type of the data.
    /// </summary>
    /// <param name="typeName">Name of the type.</param>
    /// <returns>A general data type class.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    protected static DataType GetDataType(Identifier typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        if (StringToDataTypeMap.TryGetValue(NormalizeTypeName(typeName.LocalName), out var value))
            return value;

        // a type in any other schema is user-defined -- an object type, a varray or a nested table.
        // The catalog does not say which from the column alone, so it is left unclassified rather
        // than guessed at.
        return typeName.Schema.IsNullOrWhiteSpace() ? DataType.Unknown : DataType.Other;
    }

    /// <summary>
    /// Gets the CLR type for the associated type name.
    /// </summary>
    /// <param name="typeName">A type name.</param>
    /// <returns>A CLR type for the associated database type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    protected static Type GetClrType(Identifier typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        return StringToClrTypeMap.TryGetValue(NormalizeTypeName(typeName.LocalName), out var value)
            ? value : typeof(object);
    }

    /// <summary>
    /// Quotes an identifier component.
    /// </summary>
    /// <param name="identifier">An identifier component.</param>
    /// <returns>A quoted identifier component.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="identifier"/> is empty or whitespace.</exception>
    protected static string QuoteIdentifier(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

        return "\"" + identifier + "\"";
    }

    /// <summary>
    /// Quotes a type name.
    /// </summary>
    /// <param name="name">A type name.</param>
    /// <returns>A quoted type name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />.</exception>
    protected static string QuoteName(Identifier name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var builder = StringBuilderCache.Acquire();

        if (name.Server != null)
            builder.Append(QuoteIdentifier(name.Server)).Append('.');
        if (name.Database != null)
            builder.Append(QuoteIdentifier(name.Database)).Append('.');
        if (name.Schema != null)
            builder.Append(QuoteIdentifier(name.Schema)).Append('.');
        if (name.LocalName != null)
            builder.Append(QuoteIdentifier(name.LocalName));

        return builder.GetStringAndRelease();
    }

    private static readonly FrozenSet<string> FixedLengthTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "CHAR",
        "NCHAR",
        "RAW",
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenSet<string> TypeNamesWithNoLengthAnnotation = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "BFILE",
        "BINARY_FLOAT",
        "BINARY_DOUBLE",
        "ANYDATA",
        "ANYDATASET",
        "BLOB",
        "CLOB",
        "DATE",
        "JSON",
        "LONG",
        "LONG RAW",
        "NCLOB",
        "ROWID",
        "SDO_GEOMETRY",
        "TIMESTAMP",
        "TIMESTAMP WITH LOCAL TIME ZONE",
        "TIMESTAMP WITH TIME ZONE",
        "UROWID",
        "XMLTYPE",
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, DataType> StringToDataTypeMap = new Dictionary<string, DataType>(StringComparer.OrdinalIgnoreCase)
    {
        ["BFILE"] = DataType.LargeBinary,
        ["BINARY_DOUBLE"] = DataType.Float,
        ["BINARY_FLOAT"] = DataType.Float,
        ["BINARY_INTEGER"] = DataType.BigInteger,
        ["BLOB"] = DataType.LargeBinary,
        ["BOOLEAN"] = DataType.Boolean,
        ["CHAR"] = DataType.String,
        ["ANYDATA"] = DataType.Variant,
        ["ANYDATASET"] = DataType.Variant,
        ["CLOB"] = DataType.Text,
        ["DATE"] = DataType.Date,
        ["FLOAT"] = DataType.Float,
        ["INTEGER"] = DataType.BigInteger,
        ["INTERVAL YEAR TO MONTH"] = DataType.Interval,
        ["INTERVAL DAY TO SECOND"] = DataType.Interval,
        ["JSON"] = DataType.Json,
        ["LONG"] = DataType.String,
        ["LONG RAW"] = DataType.LargeBinary,
        ["NCHAR"] = DataType.Unicode,
        ["NCLOB"] = DataType.UnicodeText,
        ["NUMBER"] = DataType.Numeric,
        ["NVARCHAR2"] = DataType.Unicode,
        ["PLS_INTEGER"] = DataType.Integer,
        ["RAW"] = DataType.Binary,
        ["REAL"] = DataType.Float,
        ["ROWID"] = DataType.Other,
        ["SDO_GEOMETRY"] = DataType.Geometry,
        ["TIMESTAMP"] = DataType.DateTime,
        ["TIMESTAMP WITH TIME ZONE"] = DataType.DateTimeOffset,
        ["TIMESTAMP WITH LOCAL TIME ZONE"] = DataType.DateTime,
        ["UROWID"] = DataType.Other,
        ["UNSIGNED INTEGER"] = DataType.BigInteger,
        ["VARCHAR2"] = DataType.String,
        ["VECTOR"] = DataType.Vector,
        ["XMLTYPE"] = DataType.Xml,
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, Type> StringToClrTypeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
    {
        ["BFILE"] = typeof(byte[]),
        ["BINARY_DOUBLE"] = typeof(double),
        ["BINARY_FLOAT"] = typeof(float),
        ["BINARY_INTEGER"] = typeof(long),
        ["BLOB"] = typeof(byte[]),
        ["BOOLEAN"] = typeof(bool),
        ["CHAR"] = typeof(string),
        ["ANYDATA"] = typeof(object),
        ["ANYDATASET"] = typeof(object),
        ["CLOB"] = typeof(string),
        ["DATE"] = typeof(DateTime),
        ["FLOAT"] = typeof(decimal),
        ["INTEGER"] = typeof(decimal),
        ["INTERVAL YEAR TO MONTH"] = typeof(int),
        ["INTERVAL DAY TO SECOND"] = typeof(TimeSpan),
        ["JSON"] = typeof(string),
        ["LONG"] = typeof(string),
        ["LONG RAW"] = typeof(byte[]),
        ["NCHAR"] = typeof(string),
        ["NCLOB"] = typeof(string),
        ["NUMBER"] = typeof(decimal),
        ["NVARCHAR2"] = typeof(string),
        ["PLS_INTEGER"] = typeof(int),
        ["RAW"] = typeof(byte[]),
        ["REAL"] = typeof(decimal),
        ["ROWID"] = typeof(string),
        ["SDO_GEOMETRY"] = typeof(object),
        ["TIMESTAMP"] = typeof(DateTime),
        ["TIMESTAMP WITH TIME ZONE"] = typeof(DateTimeOffset),
        ["TIMESTAMP WITH LOCAL TIME ZONE"] = typeof(DateTime),
        ["UROWID"] = typeof(string),
        ["UNSIGNED INTEGER"] = typeof(decimal),
        ["VARCHAR2"] = typeof(string),
        ["VECTOR"] = typeof(object),
        ["XMLTYPE"] = typeof(string),
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}