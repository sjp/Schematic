namespace SJP.Schematic.SqlServer.Queries;

/// <summary>
/// The statistics columns that are shared by the single-table and all-tables queries, so that both
/// results are mapped onto the core model by the same code.
/// </summary>
internal interface ITableStatisticsRow
{
    long? RowCount { get; }

    long? DataSizeBytes { get; }

    long? IndexSizeBytes { get; }
}
