using System;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Parsing;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// Provides column types for SQLite databases.
/// </summary>
/// <seealso cref="IDbTypeProvider" />
public class SqliteDbTypeProvider : IDbTypeProvider
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

        // a declared type is what the table definition says, so it is preferred over one derived
        // from the data type class, which can only ever name one of the five affinities
        var declaredTypeName = typeMetadata.TypeName?.LocalName;
        var typeName = declaredTypeName.IsNullOrWhiteSpace()
            ? GetDefaultTypeName(typeMetadata.DataType)
            : declaredTypeName;
        var affinity = GetAffinity(typeName);
        var collation = typeMetadata.Collation.Match(
            static c => Enum.TryParse(c.LocalName, true, out SqliteCollation sc) ? sc : SqliteCollation.None,
            static () => SqliteCollation.None
        );

        // a collation only applies to a text column, so one given for any other affinity is dropped
        return collation == SqliteCollation.None || affinity != SqliteTypeAffinity.Text
            ? new SqliteColumnType(typeName, affinity)
            : new SqliteColumnType(typeName, affinity, collation);
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

        // only interested in these two bits of information
        var typeMetadata = new ColumnTypeMetadata
        {
            Collation = otherType.Collation,
            DataType = otherType.DataType,
        };

        return CreateColumnType(typeMetadata);
    }

    /// <summary>
    /// Gets the default name of the type.
    /// </summary>
    /// <param name="dataType">The data type.</param>
    /// <returns>A type name for the given data type.</returns>
    /// <exception cref="ArgumentException"><paramref name="dataType"/> is an invalid enum value.</exception>
    protected static string GetDefaultTypeName(DataType dataType)
    {
        if (!dataType.IsValid())
            throw new ArgumentException($"The {nameof(DataType)} provided must be a valid enum.", nameof(dataType));

        return dataType switch
        {
            DataType.Binary or DataType.LargeBinary or DataType.Geometry or DataType.Bit or DataType.RowVersion or DataType.Vector => "BLOB",
            DataType.SmallInteger or DataType.BigInteger or DataType.Boolean or DataType.Integer or DataType.TinyInteger => "INTEGER",
            DataType.Float => "REAL",
            DataType.Date or DataType.DateTime or DataType.Interval or DataType.Time or DataType.Numeric or DataType.Money => "NUMERIC",
            DataType.String or DataType.Text or DataType.Unicode or DataType.UnicodeText or DataType.Json or DataType.Xml or DataType.UniqueIdentifier => "TEXT",
            // SQLite has no type of its own for any of these, and stores each of them as text
            DataType.DateTimeOffset or DataType.TimeOffset or DataType.Array or DataType.Enum or DataType.Set
                or DataType.Range or DataType.Network or DataType.Composite or DataType.Variant or DataType.FullTextSearch => "TEXT",
            _ => "NUMERIC",
        };
    }

    /// <summary>
    /// Gets the type affinity of a given type name.
    /// </summary>
    /// <param name="typeName">A type name.</param>
    /// <returns>A type affinity.</returns>
    protected static SqliteTypeAffinity GetAffinity(string typeName) => AffinityParser.ParseTypeName(typeName);

    private static readonly SqliteTypeAffinityParser AffinityParser = new();
}