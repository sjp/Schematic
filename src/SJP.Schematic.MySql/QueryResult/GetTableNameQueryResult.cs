namespace SJP.Schematic.MySql.QueryResult;

internal sealed record GetTableNameQueryResult
{
    public string SchemaName { get; init; } = default!;

    public string TableName { get; init; } = default!;
}
