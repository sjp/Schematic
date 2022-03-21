using System;
using EnumsNET;
using SJP.Schematic.Core;
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
    /// <exception cref="ArgumentNullException"><paramref name="typeMetadata"/> is <c>null</c>.</exception>
    public IDbType CreateColumnType(ColumnTypeMetadata typeMetadata)
    {
        if (typeMetadata == null)
            throw new ArgumentNullException(nameof(typeMetadata));

        var typeName = GetDefaultTypeName(typeMetadata.DataType);
        var affinity = GetAffinity(typeName);
        var collation = typeMetadata.Collation.Match(
            static c => Enum.TryParse(c.LocalName, out SqliteCollation sc) ? sc : SqliteCollation.None,
            static () => SqliteCollation.None
        );

        return collation == SqliteCollation.None
            ? new SqliteColumnType(affinity)
            : new SqliteColumnType(affinity, collation);
    }

    /// <summary>
    /// Gets the data type that most closely matches the provided data type.
    /// </summary>
    /// <param name="otherType">An data type to compare with.</param>
    /// <returns>The closest matching column data type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="otherType"/> is <c>null</c>.</exception>
    public IDbType GetComparableColumnType(IDbType otherType)
    {
        if (otherType == null)
            throw new ArgumentNullException(nameof(otherType));

        // only interested in these two bits of information
        var typeMetadata = new ColumnTypeMetadata
        {
            Collation = otherType.Collation,
            DataType = otherType.DataType
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
            throw new ArgumentException($"The { nameof(DataType) } provided must be a valid enum.", nameof(dataType));

        return dataType switch
        {
            DataType.Binary or DataType.LargeBinary => "BLOB",
            DataType.SmallInteger or DataType.BigInteger or DataType.Boolean or DataType.Integer => "INTEGER",
            DataType.Float => "REAL",
            DataType.Date or DataType.DateTime or DataType.Interval or DataType.Time or DataType.Numeric => "NUMERIC",
            DataType.String or DataType.Text or DataType.Unicode or DataType.UnicodeText => "TEXT",
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