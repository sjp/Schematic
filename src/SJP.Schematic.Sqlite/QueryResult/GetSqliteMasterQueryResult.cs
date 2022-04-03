namespace SJP.Schematic.Sqlite.QueryResult;

internal sealed record GetSqliteMasterQueryResult
{
    public string Type { get; init; } = default!;

    public string Name { get; init; } = default!;

    public string TableName { get; init; } = default!;

    public long RootPage { get; init; }

    public string Sql { get; init; } = default!;
}