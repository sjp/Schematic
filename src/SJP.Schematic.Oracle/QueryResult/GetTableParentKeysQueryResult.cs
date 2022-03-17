namespace SJP.Schematic.Oracle.QueryResult;

internal sealed record GetTableParentKeysQueryResult
{
    public string ConstraintName { get; init; } = default!;

    public string EnabledStatus { get; init; } = default!;

    public string DeleteAction { get; init; } = default!;

    public string? ParentTableSchema { get; init; }

    public string? ParentTableName { get; init; }

    public string? ParentConstraintName { get; init; }

    public string ParentKeyType { get; init; } = default!;

    public string ColumnName { get; init; } = default!;

    public int ColumnPosition { get; init; }
}
