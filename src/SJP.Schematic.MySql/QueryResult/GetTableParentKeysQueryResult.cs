namespace SJP.Schematic.MySql.QueryResult;

internal sealed record GetTableParentKeysQueryResult
{
    public string ParentTableSchema { get; init; } = default!;

    public string ParentTableName { get; init; } = default!;

    public string ChildKeyName { get; init; } = default!;

    public string ParentKeyName { get; init; } = default!;

    public string ColumnName { get; init; } = default!;

    public int ConstraintColumnId { get; init; }

    public string ParentKeyType { get; init; } = default!;

    public string DeleteAction { get; init; } = default!;

    public string UpdateAction { get; init; } = default!;
}
