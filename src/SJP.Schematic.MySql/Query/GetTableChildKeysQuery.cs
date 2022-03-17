namespace SJP.Schematic.MySql.Query;

internal sealed record GetTableChildKeysQuery
{
    public string SchemaName { get; init; } = default!;

    public string TableName { get; init; } = default!;
}
