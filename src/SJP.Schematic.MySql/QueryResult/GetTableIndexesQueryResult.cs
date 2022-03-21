namespace SJP.Schematic.MySql.QueryResult;

internal sealed record GetTableIndexesQueryResult
{
    public string IndexName { get; init; } = default!;

    public bool IsNonUnique { get; init; }

    public int ColumnOrdinal { get; init; }

    public string ColumnName { get; init; } = default!;
}