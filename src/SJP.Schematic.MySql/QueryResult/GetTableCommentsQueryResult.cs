namespace SJP.Schematic.MySql.QueryResult;

internal sealed record GetTableCommentsQueryResult
{
    public string ObjectType { get; init; } = default!;

    public string ObjectName { get; init; } = default!;

    public string? Comment { get; init; }
}
