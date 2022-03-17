namespace SJP.Schematic.PostgreSql.QueryResult;

internal sealed record GetTableUniqueKeysQueryResult
{
    public string ConstraintName { get; init; } = default!;

    public string ColumnName { get; init; } = default!;

    public int OrdinalPosition { get; init; }
}
