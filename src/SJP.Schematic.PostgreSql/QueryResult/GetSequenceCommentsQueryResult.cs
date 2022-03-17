namespace SJP.Schematic.PostgreSql.QueryResult;

internal sealed record GetSequenceCommentsQueryResult
{
    public string? Comment { get; init; }
}
