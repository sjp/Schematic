namespace SJP.Schematic.PostgreSql.QueryResult;

internal sealed record GetTableChecksQueryResult
{
    public string? ConstraintName { get; init; }

    public string? Definition { get; init; }
}
