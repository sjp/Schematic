using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Describes the size of a table's contents at a point in time, as the database records it.
/// </summary>
/// <remarks>
/// Unlike <see cref="IRelationalDatabaseTable"/>, which describes a table's structure, this
/// describes data that changes as rows are written. Values are read from whatever the engine
/// maintains in its catalog, so they are usually approximate and can be stale, or absent
/// entirely when the engine has not yet gathered statistics for the table.
/// </remarks>
public interface ITableStatistics
{
    /// <summary>
    /// The name of the table these statistics describe.
    /// </summary>
    /// <value>A table name.</value>
    Identifier TableName { get; }

    /// <summary>
    /// The number of rows in the table, when the database records one.
    /// </summary>
    /// <value>A row count, if available. Approximate unless <see cref="IsExact"/> is <see langword="true" />.</value>
    Option<long> RowCount { get; }

    /// <summary>
    /// Whether <see cref="RowCount"/> is the exact number of rows in the table rather than an estimate.
    /// </summary>
    /// <value><see langword="true" /> if the row count is exact; otherwise <see langword="false" />.</value>
    /// <remarks>
    /// Catalog-derived counts are estimates, so a provider reading from a catalog reports
    /// <see langword="false" />. Only a provider that counts rows itself can report <see langword="true" />.
    /// </remarks>
    bool IsExact { get; }

    /// <summary>
    /// The space occupied by the table's rows, in bytes, when the database records it.
    /// </summary>
    /// <value>A size in bytes, if available.</value>
    Option<long> DataSizeBytes { get; }

    /// <summary>
    /// The space occupied by the table's indexes, in bytes, when the database records it.
    /// </summary>
    /// <value>A size in bytes, if available.</value>
    Option<long> IndexSizeBytes { get; }
}
