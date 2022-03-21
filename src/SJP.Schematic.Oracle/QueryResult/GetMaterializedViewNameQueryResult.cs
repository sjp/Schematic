namespace SJP.Schematic.Oracle.QueryResult;

internal sealed record GetMaterializedViewNameQueryResult
{
    public string SchemaName { get; init; } = default!;

    public string ViewName { get; init; } = default!;
}