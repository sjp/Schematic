using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
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
        {
            typeMetadata.TypeName = GetDefaultTypeName(typeMetadata);
        }
        else
        {
            // the arguments a name carries describe the column rather than name a type, so they are
            // read out of the name and the name is left describing the type alone
            if (typeMetadata.FractionalSecondsPrecision.IsNone)
                typeMetadata.FractionalSecondsPrecision = GetFractionalSecondsPrecision(typeMetadata.TypeName.LocalName);
            typeMetadata.TypeName = NormalizeTypeName(typeMetadata.TypeName);
        }

        var typeInfo = GetTypeInfo(typeMetadata.TypeName.LocalName);
        if (typeMetadata.DataType == DataType.Unknown)
        {
            typeMetadata.DataType = GetDataType(typeMetadata.TypeName, typeInfo);
            if (typeMetadata.DataType == DataType.Numeric)
            {
                var numericPrecision = typeMetadata.NumericPrecision.MatchUnsafe(static np => np, static () => (INumericPrecision?)null);
                if (numericPrecision == null)
                {
                    typeMetadata.DataType = typeMetadata.MaxLength < 8
                        ? DataType.Integer     // 2^32
                        : DataType.BigInteger; // note: could require storing in a decimal instead of long
                }
                else if (numericPrecision.Scale == 0)
                {
                    typeMetadata.DataType = numericPrecision.Precision < 8
                        ? DataType.Integer     // 2^32
                        : DataType.BigInteger; // note: could require storing in a decimal instead of long
                }
            }
        }
        if (typeMetadata.ClrType == null)
            typeMetadata.ClrType = typeInfo?.ClrType ?? typeof(object);
        typeMetadata.IsFixedLength = typeInfo?.IsFixedLength ?? false;

        var definition = GetFormattedTypeName(typeMetadata, typeMetadata.TypeName, typeInfo);
        return _typeCache.GetOrCreate(
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
            typeMetadata.IsUnsigned,
            fractionalSecondsPrecision: typeMetadata.FractionalSecondsPrecision
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
            FractionalSecondsPrecision = otherType.FractionalSecondsPrecision,
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

        return GetTypeInfo(typeName.LocalName)?.IsFixedLength ?? false;
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
    /// Reads the fractional seconds precision out of a type name that declares one.
    /// </summary>
    /// <param name="typeName">A type name, e.g. <c>TIMESTAMP(9) WITH TIME ZONE</c>.</param>
    /// <returns>The number of digits kept after the decimal point in the seconds of a value of the type, when the name declares one; otherwise none.</returns>
    /// <remarks>
    /// A timestamp declares its precision immediately after the type, while an interval declares one
    /// for each of its fields, of which only the trailing <c>SECOND</c> has a fractional part. The
    /// leading field precision of an interval is not a fractional seconds precision, and is reported
    /// by the catalog as the type's numeric precision.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="typeName"/> is empty or whitespace.</exception>
    protected static LanguageExt.Option<int> GetFractionalSecondsPrecision(string typeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

        if (!typeName.Contains('(', StringComparison.Ordinal))
            return LanguageExt.Option<int>.None;

        var match = FractionalSecondsPrecisionRegex().Match(typeName);
        return match.Success && int.TryParse(match.Groups["precision"].ValueSpan, CultureInfo.InvariantCulture, out var precision)
            ? LanguageExt.Option<int>.Some(precision)
            : LanguageExt.Option<int>.None;
    }

    [GeneratedRegex(@"^(?:TIMESTAMP|INTERVAL\s+.*?\bSECOND)\s*\(\s*(?<precision>\d+)\s*\)", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex FractionalSecondsPrecisionRegex();

    // the arguments only ever qualify the local name, so the rest of a qualified name is untouched
    private static Identifier NormalizeTypeName(Identifier typeName)
    {
        var localName = NormalizeTypeName(typeName.LocalName);
        return string.Equals(localName, typeName.LocalName, StringComparison.Ordinal)
            ? typeName
            : Identifier.CreateQualifiedIdentifier(typeName.Server, typeName.Database, typeName.Schema, localName);
    }

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

        return GetFormattedTypeName(typeMetadata, typeMetadata.TypeName, GetTypeInfo(typeMetadata.TypeName.LocalName));
    }

    private static string GetFormattedTypeName(ColumnTypeMetadata typeMetadata, Identifier typeName, TypeInfo? typeInfo)
    {
        var builder = StringBuilderCache.Acquire(typeName.LocalName.Length * 2);
        if (string.Equals(typeName.Schema, "SYS", StringComparison.OrdinalIgnoreCase))
            AppendQuotedIdentifier(builder, typeName.LocalName);
        else
            AppendQuotedName(builder, typeName);

        var annotation = typeInfo?.Annotation ?? TypeAnnotation.LengthOrPrecision;

        // a timestamp or a day-to-second interval is annotated with the precision of its seconds and
        // with nothing else; its numeric precision, where it has one, counts the leading field's digits
        if (annotation == TypeAnnotation.FractionalSecondsPrecision)
        {
            var fractionalSecondsPrecision = typeMetadata.FractionalSecondsPrecision.MatchUnsafe(static p => p, static () => (int?)null);
            if (fractionalSecondsPrecision is int precision)
                builder.Append(CultureInfo.InvariantCulture, $"({precision})");

            return builder.GetStringAndRelease();
        }

        if (annotation == TypeAnnotation.None)
            return builder.GetStringAndRelease();

        var numericPrecision = typeMetadata.NumericPrecision.MatchUnsafe(static np => np, static () => (INumericPrecision?)null);
        if (numericPrecision != null && (numericPrecision.Precision > 0 || numericPrecision.Scale > 0))
        {
            builder.Append(CultureInfo.InvariantCulture, $"({numericPrecision.Precision}");
            if (numericPrecision.Scale > 0)
                builder.Append(CultureInfo.InvariantCulture, $", {numericPrecision.Scale}");
            builder.Append(')');
        }
        else if (typeMetadata.MaxLength > 0)
        {
            builder.Append(CultureInfo.InvariantCulture, $"({typeMetadata.MaxLength})");
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

        return GetDataType(typeName, GetTypeInfo(typeName.LocalName));
    }

    private static DataType GetDataType(Identifier typeName, TypeInfo? typeInfo)
    {
        if (typeInfo != null)
            return typeInfo.DataType;

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

        return GetTypeInfo(typeName.LocalName)?.ClrType ?? typeof(object);
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
        AppendQuotedName(builder, name);
        return builder.GetStringAndRelease();
    }

    private static void AppendQuotedName(StringBuilder builder, Identifier name)
    {
        if (name.Server != null)
            AppendQuotedIdentifier(builder, name.Server).Append('.');
        if (name.Database != null)
            AppendQuotedIdentifier(builder, name.Database).Append('.');
        if (name.Schema != null)
            AppendQuotedIdentifier(builder, name.Schema).Append('.');
        AppendQuotedIdentifier(builder, name.LocalName);
    }

    private static StringBuilder AppendQuotedIdentifier(StringBuilder builder, string identifier)
    {
        return builder.Append('"').Append(identifier).Append('"');
    }

    // built-in types are recognised by name alone, whichever schema the name is qualified with
    private static TypeInfo? GetTypeInfo(string localName)
    {
        return Types.TryGetValue(NormalizeTypeName(localName), out var typeInfo)
            ? typeInfo
            : null;
    }

    private enum TypeAnnotation
    {
        // the length, or the precision and scale, in parentheses
        LengthOrPrecision,

        // nothing, the type takes no arguments
        None,

        // the fractional seconds precision in parentheses, when there is one
        FractionalSecondsPrecision,
    }

    private sealed record TypeInfo(DataType DataType, Type ClrType, TypeAnnotation Annotation, bool IsFixedLength);

    private static readonly FrozenDictionary<string, TypeInfo> Types = new Dictionary<string, TypeInfo>(StringComparer.OrdinalIgnoreCase)
    {
        ["ANYDATA"] = new(DataType.Variant, typeof(object), TypeAnnotation.None, IsFixedLength: false),
        ["ANYDATASET"] = new(DataType.Variant, typeof(object), TypeAnnotation.None, IsFixedLength: false),
        ["BFILE"] = new(DataType.LargeBinary, typeof(byte[]), TypeAnnotation.None, IsFixedLength: false),
        ["BINARY_DOUBLE"] = new(DataType.Float, typeof(double), TypeAnnotation.None, IsFixedLength: false),
        ["BINARY_FLOAT"] = new(DataType.Float, typeof(float), TypeAnnotation.None, IsFixedLength: false),
        ["BINARY_INTEGER"] = new(DataType.BigInteger, typeof(long), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["BLOB"] = new(DataType.LargeBinary, typeof(byte[]), TypeAnnotation.None, IsFixedLength: false),
        ["BOOLEAN"] = new(DataType.Boolean, typeof(bool), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["CHAR"] = new(DataType.String, typeof(string), TypeAnnotation.LengthOrPrecision, IsFixedLength: true),
        ["CLOB"] = new(DataType.Text, typeof(string), TypeAnnotation.None, IsFixedLength: false),
        ["DATE"] = new(DataType.Date, typeof(DateTime), TypeAnnotation.None, IsFixedLength: false),
        ["FLOAT"] = new(DataType.Float, typeof(decimal), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["INTEGER"] = new(DataType.BigInteger, typeof(decimal), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["INTERVAL DAY TO SECOND"] = new(DataType.Interval, typeof(TimeSpan), TypeAnnotation.FractionalSecondsPrecision, IsFixedLength: false),
        ["INTERVAL YEAR TO MONTH"] = new(DataType.Interval, typeof(int), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["JSON"] = new(DataType.Json, typeof(string), TypeAnnotation.None, IsFixedLength: false),
        ["LONG"] = new(DataType.String, typeof(string), TypeAnnotation.None, IsFixedLength: false),
        ["LONG RAW"] = new(DataType.LargeBinary, typeof(byte[]), TypeAnnotation.None, IsFixedLength: false),
        ["NCHAR"] = new(DataType.Unicode, typeof(string), TypeAnnotation.LengthOrPrecision, IsFixedLength: true),
        ["NCLOB"] = new(DataType.UnicodeText, typeof(string), TypeAnnotation.None, IsFixedLength: false),
        ["NUMBER"] = new(DataType.Numeric, typeof(decimal), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["NVARCHAR2"] = new(DataType.Unicode, typeof(string), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["PLS_INTEGER"] = new(DataType.Integer, typeof(int), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["RAW"] = new(DataType.Binary, typeof(byte[]), TypeAnnotation.LengthOrPrecision, IsFixedLength: true),
        ["REAL"] = new(DataType.Float, typeof(decimal), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["ROWID"] = new(DataType.Other, typeof(string), TypeAnnotation.None, IsFixedLength: false),
        ["SDO_GEOMETRY"] = new(DataType.Geometry, typeof(object), TypeAnnotation.None, IsFixedLength: false),
        ["TIMESTAMP"] = new(DataType.DateTime, typeof(DateTime), TypeAnnotation.FractionalSecondsPrecision, IsFixedLength: false),
        ["TIMESTAMP WITH LOCAL TIME ZONE"] = new(DataType.DateTime, typeof(DateTime), TypeAnnotation.FractionalSecondsPrecision, IsFixedLength: false),
        ["TIMESTAMP WITH TIME ZONE"] = new(DataType.DateTimeOffset, typeof(DateTimeOffset), TypeAnnotation.FractionalSecondsPrecision, IsFixedLength: false),
        ["UNSIGNED INTEGER"] = new(DataType.BigInteger, typeof(decimal), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["UROWID"] = new(DataType.Other, typeof(string), TypeAnnotation.None, IsFixedLength: false),
        ["VARCHAR2"] = new(DataType.String, typeof(string), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["VECTOR"] = new(DataType.Vector, typeof(object), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["XMLTYPE"] = new(DataType.Xml, typeof(string), TypeAnnotation.None, IsFixedLength: false),
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    // columns repeat a few types many times over, so identical types are shared rather than each column holding its own copy
    private readonly DbTypeCache _typeCache = new();
}