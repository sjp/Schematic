namespace SJP.Schematic.Sqlite.QueryResult;

internal sealed record GetAllTableNamesQueryResult
{
    public string TableName { get; set; } = default!;
}