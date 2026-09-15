using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// A database column type provider for SQL Server column types.
/// </summary>
/// <seealso cref="IDbTypeProvider" />
public class SqlServerDbTypeProvider : IDbTypeProvider
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

        var typeInfo = GetSystemTypeInfo(typeMetadata.TypeName);
        if (typeMetadata.DataType == DataType.Unknown)
            typeMetadata.DataType = typeInfo?.DataType ?? DataType.Unknown;
        if (typeMetadata.ClrType == null)
            typeMetadata.ClrType = typeInfo?.ClrType ?? typeof(object);
        typeMetadata.IsFixedLength = typeInfo?.IsFixedLength ?? false;

        // varbinary(max) has no declared length, and is a large object rather than an inline value
        if (typeMetadata.DataType == DataType.Binary && typeMetadata.MaxLength <= 0)
            typeMetadata.DataType = DataType.LargeBinary;

        if (typeMetadata.FractionalSecondsPrecision.IsNone)
            typeMetadata.FractionalSecondsPrecision = GetFractionalSecondsPrecision(typeInfo, typeMetadata.NumericPrecision);

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
    /// Determines whether the data type is required to be of fixed length.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns><see langword="true" /> if the data type must be a fixed length.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    protected static bool GetIsFixedLength(Identifier typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        return GetSystemTypeInfo(typeName)?.IsFixedLength ?? false;
    }

    /// <summary>
    /// Gets the default name of the type, given sufficient metadata.
    /// </summary>
    /// <param name="typeMetadata">Column type metadata.</param>
    /// <returns>A type name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeMetadata"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a data type was unable to be parsed.</exception>
    protected static Identifier GetDefaultTypeName(ColumnTypeMetadata typeMetadata)
    {
        ArgumentNullException.ThrowIfNull(typeMetadata);

        return typeMetadata.DataType switch
        {
            DataType.BigInteger => new Identifier("sys", "bigint"),
            DataType.Binary or DataType.LargeBinary => typeMetadata.IsFixedLength
                ? new Identifier("sys", "binary")
                : new Identifier("sys", "varbinary"),
            DataType.Boolean => new Identifier("sys", "bit"),
            DataType.Date or DataType.DateTime => new Identifier("sys", "datetime2"),
            DataType.DateTimeOffset or DataType.TimeOffset => new Identifier("sys", "datetimeoffset"),
            DataType.Float => new Identifier("sys", "float"),
            DataType.Geometry => new Identifier("sys", "geometry"),
            DataType.Integer => new Identifier("sys", "int"),
            // SQL Server has no interval type; a duration is stored as a time of day, which is
            // the only type it has that measures anything other than a point in time
            DataType.Interval => new Identifier("sys", "time"),
            DataType.Json => new Identifier("sys", "json"),
            DataType.Money => new Identifier("sys", "money"),
            DataType.Numeric => new Identifier("sys", "numeric"),
            DataType.RowVersion => new Identifier("sys", "rowversion"),
            DataType.SmallInteger => new Identifier("sys", "smallint"),
            DataType.String or DataType.Text => typeMetadata.IsFixedLength
                ? new Identifier("sys", "char")
                : new Identifier("sys", "varchar"),
            DataType.Time => new Identifier("sys", "time"),
            DataType.TinyInteger => new Identifier("sys", "tinyint"),
            DataType.Unicode or DataType.UnicodeText => typeMetadata.IsFixedLength
                ? new Identifier("sys", "nchar")
                : new Identifier("sys", "nvarchar"),
            DataType.UniqueIdentifier => new Identifier("sys", "uniqueidentifier"),
            DataType.Vector => new Identifier("sys", "vector"),
            DataType.Xml => new Identifier("sys", "xml"),
            // SQL Server has no bit-string type; a run of bits is stored as binary
            DataType.Bit => new Identifier("sys", "varbinary"),
            // enumerated, set and network values have no type of their own, and are stored as text
            DataType.Enum or DataType.Set or DataType.Network or DataType.FullTextSearch => new Identifier("sys", "nvarchar"),
            // sql_variant is the only type able to hold a value whose shape SQL Server cannot describe
            DataType.Array or DataType.Range or DataType.Composite or DataType.Variant or DataType.Other => new Identifier("sys", "sql_variant"),
            DataType.Unknown => throw new ArgumentOutOfRangeException(nameof(typeMetadata), "Unable to determine a type name for an unknown data type."),
            _ => throw new ArgumentOutOfRangeException(nameof(typeMetadata), "Unable to determine a type name for data type: " + typeMetadata.DataType.ToString()),
        };
    }

    /// <summary>
    /// Gets the fractional seconds precision that a temporal type declares.
    /// </summary>
    /// <param name="typeName">A type name.</param>
    /// <param name="numericPrecision">The precision and scale the catalog reports for the column.</param>
    /// <returns>The number of digits kept after the decimal point in the seconds of a value of the type, for a temporal type; otherwise none.</returns>
    /// <remarks>
    /// SQL Server describes a temporal column with the same precision and scale columns it uses for a
    /// numeric one, where the scale is the fractional seconds precision. The scale means nothing of
    /// the sort for any other type, so only the temporal types report one.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    protected static LanguageExt.Option<int> GetFractionalSecondsPrecision(Identifier typeName, LanguageExt.Option<INumericPrecision> numericPrecision)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        return GetFractionalSecondsPrecision(GetSystemTypeInfo(typeName), numericPrecision);
    }

    private static LanguageExt.Option<int> GetFractionalSecondsPrecision(SystemTypeInfo? typeInfo, LanguageExt.Option<INumericPrecision> numericPrecision)
    {
        return typeInfo?.Annotation == TypeAnnotation.FractionalSecondsPrecision
            ? numericPrecision.Map(static np => np.Scale)
            : LanguageExt.Option<int>.None;
    }

    /// <summary>
    /// Gets the name of the formatted type.
    /// </summary>
    /// <param name="typeMetadata">Column type metadata.</param>
    /// <returns>A formatted type name, sufficient for printing or use within queries.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeMetadata"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="typeMetadata"/> does not have a type name.</exception>
    protected static string GetFormattedTypeName(ColumnTypeMetadata typeMetadata)
    {
        ArgumentNullException.ThrowIfNull(typeMetadata);
        if (typeMetadata.TypeName == null)
            throw new ArgumentException("The type name is missing. A formatted type name cannot be generated.", nameof(typeMetadata));

        return GetFormattedTypeName(typeMetadata, typeMetadata.TypeName, GetSystemTypeInfo(typeMetadata.TypeName));
    }

    private static string GetFormattedTypeName(ColumnTypeMetadata typeMetadata, Identifier typeName, SystemTypeInfo? typeInfo)
    {
        var builder = StringBuilderCache.Acquire(typeName.LocalName.Length * 2);
        if (string.Equals(typeName.Schema, "sys", StringComparison.OrdinalIgnoreCase))
            AppendQuotedIdentifier(builder, typeName.LocalName);
        else
            AppendQuotedName(builder, typeName);

        var annotation = typeInfo?.Annotation ?? TypeAnnotation.LengthOrPrecision;
        if (annotation == TypeAnnotation.None)
            return builder.GetStringAndRelease();

        // a temporal type is annotated with the precision of its seconds; its precision and scale
        // describe the same thing twice, so printing both would name a type that does not exist
        if (annotation == TypeAnnotation.FractionalSecondsPrecision)
        {
            var fractionalSecondsPrecision = typeMetadata.FractionalSecondsPrecision.MatchUnsafe(static p => p, static () => (int?)null);
            if (fractionalSecondsPrecision is int precision)
                builder.Append(CultureInfo.InvariantCulture, $"({precision})");

            return builder.GetStringAndRelease();
        }

        builder.Append('(');

        var numericPrecision = typeMetadata.NumericPrecision.MatchUnsafe(static np => np, static () => (INumericPrecision?)null);
        if (numericPrecision != null && (numericPrecision.Precision > 0 || numericPrecision.Scale > 0))
        {
            builder.Append(CultureInfo.InvariantCulture, $"{numericPrecision.Precision}");
            if (numericPrecision.Scale > 0)
                builder.Append(CultureInfo.InvariantCulture, $", {numericPrecision.Scale}");
        }
        else if (typeMetadata.MaxLength > 0)
        {
            var maxLength = typeMetadata.DataType == DataType.Unicode || typeMetadata.DataType == DataType.UnicodeText
                ? typeMetadata.MaxLength / 2
                : typeMetadata.MaxLength;

            builder.Append(CultureInfo.InvariantCulture, $"{maxLength}");
        }
        else
        {
            builder.Append("max");
        }

        builder.Append(')');

        return builder.GetStringAndRelease();
    }

    /// <summary>
    /// Gets the data type for an associated type name.
    /// </summary>
    /// <param name="typeName">A type name.</param>
    /// <returns>A data type definition.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    protected static DataType GetDataType(Identifier typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        return GetSystemTypeInfo(typeName)?.DataType ?? DataType.Unknown;
    }

    /// <summary>
    /// Gets the CLR type for the associated type name.
    /// </summary>
    /// <param name="typeName">A type name.</param>
    /// <returns>A CLR type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    protected static Type GetClrType(Identifier typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        return GetSystemTypeInfo(typeName)?.ClrType ?? typeof(object);
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

        return $"[{identifier.Replace("]", "]]", StringComparison.Ordinal)}]";
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
        builder.Append('[');

        var remaining = identifier.AsSpan();
        int closingBracketIndex;
        while ((closingBracketIndex = remaining.IndexOf(']')) >= 0)
        {
            builder.Append(remaining[..(closingBracketIndex + 1)]).Append(']');
            remaining = remaining[(closingBracketIndex + 1)..];
        }

        return builder.Append(remaining).Append(']');
    }

    // the built-in types are the only ones described here, and they are only ever named by an unqualified sys schema
    private static SystemTypeInfo? GetSystemTypeInfo(Identifier typeName)
    {
        return typeName.Server == null
            && typeName.Database == null
            && string.Equals(typeName.Schema, "sys", StringComparison.OrdinalIgnoreCase)
            && SystemTypes.TryGetValue(typeName.LocalName, out var typeInfo)
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

    private sealed record SystemTypeInfo(DataType DataType, Type ClrType, TypeAnnotation Annotation, bool IsFixedLength);

    // datetime and smalldatetime are not annotated with a fractional seconds precision deliberately: their resolution
    // is fixed by the type rather than declared with the column, so there is no precision of the column's own to report
    private static readonly FrozenDictionary<string, SystemTypeInfo> SystemTypes = new Dictionary<string, SystemTypeInfo>(StringComparer.OrdinalIgnoreCase)
    {
        ["bigint"] = new(DataType.BigInteger, typeof(long), TypeAnnotation.None, IsFixedLength: false),
        ["binary"] = new(DataType.Binary, typeof(byte[]), TypeAnnotation.LengthOrPrecision, IsFixedLength: true),
        ["bit"] = new(DataType.Boolean, typeof(bool), TypeAnnotation.None, IsFixedLength: false),
        ["char"] = new(DataType.String, typeof(string), TypeAnnotation.LengthOrPrecision, IsFixedLength: true),
        ["date"] = new(DataType.Date, typeof(DateTime), TypeAnnotation.None, IsFixedLength: false),
        ["datetime"] = new(DataType.DateTime, typeof(DateTime), TypeAnnotation.None, IsFixedLength: false),
        ["datetime2"] = new(DataType.DateTime, typeof(DateTime), TypeAnnotation.FractionalSecondsPrecision, IsFixedLength: false),
        ["datetimeoffset"] = new(DataType.DateTimeOffset, typeof(DateTimeOffset), TypeAnnotation.FractionalSecondsPrecision, IsFixedLength: false),
        ["decimal"] = new(DataType.Numeric, typeof(decimal), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["float"] = new(DataType.Float, typeof(double), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["geography"] = new(DataType.Geometry, typeof(object), TypeAnnotation.None, IsFixedLength: false),
        ["geometry"] = new(DataType.Geometry, typeof(object), TypeAnnotation.None, IsFixedLength: false),
        ["hierarchyid"] = new(DataType.Other, typeof(object), TypeAnnotation.None, IsFixedLength: false),
        ["image"] = new(DataType.LargeBinary, typeof(byte[]), TypeAnnotation.None, IsFixedLength: false),
        ["int"] = new(DataType.Integer, typeof(int), TypeAnnotation.None, IsFixedLength: false),
        ["json"] = new(DataType.Json, typeof(string), TypeAnnotation.None, IsFixedLength: false),
        ["money"] = new(DataType.Money, typeof(decimal), TypeAnnotation.None, IsFixedLength: false),
        ["nchar"] = new(DataType.Unicode, typeof(string), TypeAnnotation.LengthOrPrecision, IsFixedLength: true),
        ["ntext"] = new(DataType.UnicodeText, typeof(string), TypeAnnotation.None, IsFixedLength: false),
        ["numeric"] = new(DataType.Numeric, typeof(decimal), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["nvarchar"] = new(DataType.Unicode, typeof(string), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["real"] = new(DataType.Float, typeof(float), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["rowversion"] = new(DataType.RowVersion, typeof(byte[]), TypeAnnotation.None, IsFixedLength: false),
        ["smalldatetime"] = new(DataType.DateTime, typeof(DateTime), TypeAnnotation.None, IsFixedLength: false),
        ["smallint"] = new(DataType.SmallInteger, typeof(short), TypeAnnotation.None, IsFixedLength: false),
        ["smallmoney"] = new(DataType.Money, typeof(decimal), TypeAnnotation.None, IsFixedLength: false),
        ["sql_variant"] = new(DataType.Variant, typeof(object), TypeAnnotation.None, IsFixedLength: false),
        ["sysname"] = new(DataType.Unicode, typeof(string), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["text"] = new(DataType.Text, typeof(string), TypeAnnotation.None, IsFixedLength: false),
        ["time"] = new(DataType.Time, typeof(TimeSpan), TypeAnnotation.FractionalSecondsPrecision, IsFixedLength: false),
        ["timestamp"] = new(DataType.RowVersion, typeof(byte[]), TypeAnnotation.None, IsFixedLength: false),
        ["tinyint"] = new(DataType.TinyInteger, typeof(byte), TypeAnnotation.None, IsFixedLength: false),
        ["uniqueidentifier"] = new(DataType.UniqueIdentifier, typeof(Guid), TypeAnnotation.None, IsFixedLength: false),
        ["varbinary"] = new(DataType.Binary, typeof(byte[]), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["varchar"] = new(DataType.String, typeof(string), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["vector"] = new(DataType.Vector, typeof(object), TypeAnnotation.LengthOrPrecision, IsFixedLength: false),
        ["xml"] = new(DataType.Xml, typeof(string), TypeAnnotation.None, IsFixedLength: false),
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    // columns repeat a few types many times over, so identical types are shared rather than each column holding its own copy
    private readonly DbTypeCache _typeCache = new();
}