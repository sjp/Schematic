namespace SJP.Schematic.MySql.QueryResult;

internal sealed record GetTableCheckConstraintsQueryResult
{
    public string ConstraintName { get; init; } = default!;

    public string Definition { get; init; } = default!;

    public string Enforced { get; init; } = default!;
}