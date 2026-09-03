using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.MySql;

/// <summary>
/// A database type provider for MySQL.
/// </summary>
/// <seealso cref="IDbTypeProvider" />
public class MySqlDbTypeProvider : IDbTypeProvider
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
            typeMetadata.DataType = GetDataType(typeMetadata.TypeName.LocalName);
        if (typeMetadata.ClrType == null)
            typeMetadata.ClrType = GetClrType(typeMetadata.TypeName.LocalName);
        typeMetadata.IsFixedLength = GetIsFixedLength(typeMetadata.TypeName.LocalName);

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
    /// <exception cref="ArgumentException"><paramref name="typeName"/> is empty or whitespace.</exception>
    protected static bool GetIsFixedLength(string typeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

        return FixedLengthTypes.Contains(typeName);
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
            DataType.BigInteger => "bigint",
            DataType.Binary => typeMetadata.IsFixedLength ? "binary" : "varbinary",
            DataType.LargeBinary => "longblob",
            // MySQL has no boolean type; BOOL is a synonym for TINYINT(1).
            DataType.Boolean => "tinyint",
            DataType.Bit => "bit",
            DataType.Date => "date",
            DataType.DateTime => "datetime",
            // MySQL has no offset-aware type; a TIMESTAMP is stored in UTC and read back in the
            // session time zone, which is as close as the server comes to carrying an offset.
            DataType.DateTimeOffset or DataType.TimeOffset => "timestamp",
            DataType.Enum => "enum",
            DataType.Float => "double",
            DataType.Geometry => "geometry",
            DataType.Integer => "int",
            DataType.Interval => "time",
            DataType.Json => "json",
            DataType.Money or DataType.Numeric => "decimal",
            DataType.Set => "set",
            DataType.SmallInteger => "smallint",
            DataType.String or DataType.Unicode => typeMetadata.IsFixedLength ? "char" : "varchar",
            DataType.Text or DataType.UnicodeText => "longtext",
            DataType.Time => "time",
            DataType.TinyInteger => "tinyint",
            // MySQL has no native GUID type; UUIDs are conventionally stored as CHAR(36).
            DataType.UniqueIdentifier => "char",
            // MySQL has no native XML type; XML documents are stored as unbounded text.
            DataType.Xml => "longtext",
            // a row version and a vector are both opaque runs of bytes.
            DataType.RowVersion or DataType.Vector => "varbinary",
            // MySQL has no type of its own for any of these, and stores each of them as text.
            DataType.Array or DataType.Range or DataType.Composite or DataType.Network
                or DataType.Variant or DataType.FullTextSearch or DataType.Other => "longtext",
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
        var typeName = typeMetadata.TypeName.LocalName;

        builder.Append(typeName);

        // an enum or a set is declared by its members rather than by a length
        if (typeMetadata.EnumValues.Count > 0)
        {
            builder.Append('(');
            for (var i = 0; i < typeMetadata.EnumValues.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                builder.Append('\'').Append(typeMetadata.EnumValues[i].Replace("'", "''", StringComparison.Ordinal)).Append('\'');
            }
            builder.Append(')');
        }
        else if (!TypeNamesWithNoLengthAnnotation.Contains(typeName))
        {
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
            // an unbounded type carries no annotation at all -- an empty '()' is not valid syntax
            else if (typeMetadata.MaxLength > 0)
            {
                builder.Append('(');
                builder.Append(typeMetadata.MaxLength.ToString(CultureInfo.InvariantCulture));
                builder.Append(')');
            }
        }

        if (typeMetadata.IsUnsigned)
            builder.Append(" unsigned");

        return builder.GetStringAndRelease();
    }

    /// <summary>
    /// Gets the type of the data.
    /// </summary>
    /// <param name="typeName">Name of the type.</param>
    /// <returns>A general data type class.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="typeName"/> is empty or whitespace.</exception>
    protected static DataType GetDataType(string typeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

        return StringToDataTypeMap.TryGetValue(typeName, out var dataType)
            ? dataType
            : DataType.Unknown;
    }

    /// <summary>
    /// Gets the CLR type for the associated type name.
    /// </summary>
    /// <param name="typeName">A type name.</param>
    /// <returns>A CLR type for the associated database type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="typeName"/> is empty or whitespace.</exception>
    protected static Type GetClrType(string typeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

        return StringToClrTypeMap.TryGetValue(typeName, out var clrType)
            ? clrType
            : typeof(object);
    }

    private static readonly FrozenSet<string> FixedLengthTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "char",
        "binary",
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenSet<string> TypeNamesWithNoLengthAnnotation = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "bit",
        "tinyint",
        "smallint",
        "mediumint",
        "int",
        "bigint",
        "double",
        "tinyblob",
        "blob",
        "mediumblob",
        "longblob",
        "tinytext",
        "text",
        "mediumtext",
        "longtext",
        "date",
        "json",
        "geometry",
        "point",
        "linestring",
        "polygon",
        "multipoint",
        "multilinestring",
        "multipolygon",
        "geometrycollection",
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, DataType> StringToDataTypeMap = new Dictionary<string, DataType>(StringComparer.OrdinalIgnoreCase)
    {
        ["bit"] = DataType.Bit,
        ["tinyint"] = DataType.TinyInteger,
        ["smallint"] = DataType.SmallInteger,
        ["mediumint"] = DataType.Integer,
        ["int"] = DataType.Integer,
        ["bigint"] = DataType.BigInteger,
        ["numeric"] = DataType.Numeric,
        ["decimal"] = DataType.Numeric,
        ["float"] = DataType.Float,
        ["double"] = DataType.Float,
        ["date"] = DataType.Date,
        ["datetime"] = DataType.DateTime,
        ["timestamp"] = DataType.DateTime,
        ["time"] = DataType.Time,
        ["year"] = DataType.SmallInteger,
        ["char"] = DataType.Unicode,
        ["varchar"] = DataType.Unicode,
        ["enum"] = DataType.Enum,
        ["set"] = DataType.Set,
        ["binary"] = DataType.Binary,
        ["varbinary"] = DataType.Binary,
        ["tinyblob"] = DataType.LargeBinary,
        ["blob"] = DataType.LargeBinary,
        ["mediumblob"] = DataType.LargeBinary,
        ["longblob"] = DataType.LargeBinary,
        ["tinytext"] = DataType.UnicodeText,
        ["text"] = DataType.UnicodeText,
        ["mediumtext"] = DataType.UnicodeText,
        ["longtext"] = DataType.UnicodeText,
        ["json"] = DataType.Json,
        ["geometry"] = DataType.Geometry,
        ["point"] = DataType.Geometry,
        ["linestring"] = DataType.Geometry,
        ["polygon"] = DataType.Geometry,
        ["multipoint"] = DataType.Geometry,
        ["multilinestring"] = DataType.Geometry,
        ["multipolygon"] = DataType.Geometry,
        ["geometrycollection"] = DataType.Geometry,
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, Type> StringToClrTypeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
    {
        ["bit"] = typeof(ulong),
        ["tinyint"] = typeof(byte),
        ["smallint"] = typeof(short),
        ["mediumint"] = typeof(int),
        ["int"] = typeof(int),
        ["bigint"] = typeof(long),
        ["decimal"] = typeof(decimal),
        ["numeric"] = typeof(decimal),
        ["float"] = typeof(double),
        ["double"] = typeof(double),
        ["date"] = typeof(DateTime),
        ["datetime"] = typeof(DateTime),
        ["timestamp"] = typeof(DateTime),
        ["time"] = typeof(TimeSpan),
        ["year"] = typeof(short),
        ["char"] = typeof(string),
        ["varchar"] = typeof(string),
        ["enum"] = typeof(string),
        ["set"] = typeof(string),
        ["tinytext"] = typeof(string),
        ["text"] = typeof(string),
        ["mediumtext"] = typeof(string),
        ["longtext"] = typeof(string),
        ["binary"] = typeof(byte[]),
        ["varbinary"] = typeof(byte[]),
        ["tinyblob"] = typeof(byte[]),
        ["blob"] = typeof(byte[]),
        ["mediumblob"] = typeof(byte[]),
        ["longblob"] = typeof(byte[]),
        ["json"] = typeof(string),
        ["geometry"] = typeof(object),
        ["point"] = typeof(object),
        ["linestring"] = typeof(object),
        ["polygon"] = typeof(object),
        ["multipoint"] = typeof(object),
        ["multilinestring"] = typeof(object),
        ["multipolygon"] = typeof(object),
        ["geometrycollection"] = typeof(object),
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}