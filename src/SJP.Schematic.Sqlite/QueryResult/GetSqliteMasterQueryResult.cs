namespace SJP.Schematic.Sqlite.QueryResult;

internal sealed record GetSqliteMasterQueryResult
{
    public string type { get; init; } = default!;

    public string name { get; init; } = default!;

    public string tbl_name { get; init; } = default!;

    public long rootpage { get; init; }

    public string sql { get; init; } = default!;
}
