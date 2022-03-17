namespace SJP.Schematic.PostgreSql.QueryResult;

internal sealed record GetSequenceNameQueryResult
{
    public string SchemaName { get; init; } = default!;

    public string SequenceName { get; init; } = default!;
}
