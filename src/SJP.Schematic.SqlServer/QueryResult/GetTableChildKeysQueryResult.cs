namespace SJP.Schematic.SqlServer.QueryResult;

internal sealed record GetTableChildKeysQueryResult
{
    public string ChildTableSchema { get; init; } = default!;

    public string ChildTableName { get; init; } = default!;

    public string ChildKeyName { get; init; } = default!;

    public string ParentKeyName { get; init; } = default!;

    public string ParentKeyType { get; init; } = default!;

    public int DeleteAction { get; init; }

    public int UpdateAction { get; init; }
}
