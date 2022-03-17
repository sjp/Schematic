namespace SJP.Schematic.PostgreSql.QueryResult;

internal sealed record GetTableCommentsQueryResult
{
    public string? ObjectType { get; init; }

    public string? ObjectName { get; init; }

    public string? Comment { get; init; }
}
