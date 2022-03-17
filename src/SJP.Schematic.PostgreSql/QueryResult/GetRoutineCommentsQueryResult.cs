namespace SJP.Schematic.PostgreSql.QueryResult;

internal sealed record GetRoutineCommentsQueryResult
{
    public string? Comment { get; init; }
}
