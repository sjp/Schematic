namespace SJP.Schematic.PostgreSql.Query;

internal sealed record GetMaterializedViewColumnsQuery
{
    public string SchemaName { get; init; } = default!;

    public string ViewName { get; init; } = default!;
}