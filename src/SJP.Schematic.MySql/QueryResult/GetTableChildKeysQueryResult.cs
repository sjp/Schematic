namespace SJP.Schematic.MySql.QueryResult;

internal sealed record GetTableChildKeysQueryResult
{
    public string ChildTableSchema { get; init; } = default!;

    public string ChildTableName { get; init; } = default!;

    public string ChildKeyName { get; init; } = default!;

    public string ParentKeyName { get; init; } = default!;

    public string ParentKeyType { get; init; } = default!;

    public string DeleteAction { get; init; } = default!;

    public string UpdateAction { get; init; } = default!;
}