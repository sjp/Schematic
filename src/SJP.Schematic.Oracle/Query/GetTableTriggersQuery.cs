namespace SJP.Schematic.Oracle.Query;

internal sealed record GetTableTriggersQuery
{
    public string SchemaName { get; init; } = default!;

    public string TableName { get; init; } = default!;
}