namespace SJP.Schematic.PostgreSql.QueryResult;

internal sealed record GetViewCommentsQueryResult
{
    public string? ObjectType { get; init; }

    public string? ObjectName { get; init; }

    public string? Comment { get; init; }
}