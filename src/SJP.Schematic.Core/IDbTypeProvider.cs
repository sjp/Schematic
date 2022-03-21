namespace SJP.Schematic.Core;

/// <summary>
/// Defines a column data type provider.
/// </summary>
public interface IDbTypeProvider
{
    /// <summary>
    /// Creates a column data type based on provided metadata.
    /// </summary>
    /// <param name="typeMetadata">Column type metadata.</param>
    /// <returns>A column data type.</returns>
    IDbType CreateColumnType(ColumnTypeMetadata typeMetadata);

    /// <summary>
    /// Gets the data type that most closely matches the provided data type.
    /// </summary>
    /// <param name="otherType">An data type to compare with.</param>
    /// <returns>The closest matching column data type.</returns>
    IDbType GetComparableColumnType(IDbType otherType);
}