namespace SJP.Schematic.PostgreSql.QueryResult;

internal sealed record GetTableIndexesQueryResult
{
    public string? IndexName { get; init; }

    public bool IsUnique { get; init; }

    public bool IsPrimary { get; init; }

    public int KeyColumnCount { get; init; }

    public int IndexColumnId { get; init; }

    public string? IndexColumnExpression { get; init; }

    public bool IsDescending { get; init; }

    public bool IsFunctional { get; init; }
}