namespace SJP.Schematic.Oracle.QueryResult;

internal sealed record GetViewNameQueryResult
{
    public string SchemaName { get; init; } = default!;

    public string ViewName { get; init; } = default!;
}
