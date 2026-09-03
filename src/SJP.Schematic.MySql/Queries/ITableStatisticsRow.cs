namespace SJP.Schematic.MySql.Queries;

/// <summary>
/// The statistics columns that are shared by the single-table and all-tables queries, so that both
/// results are mapped onto the core model by the same code.
/// </summary>
internal interface ITableStatisticsRow
{
    ulong? RowCount { get; }

    ulong? DataSizeBytes { get; }

    ulong? IndexSizeBytes { get; }
}
