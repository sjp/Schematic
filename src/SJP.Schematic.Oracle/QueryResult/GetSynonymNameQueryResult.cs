namespace SJP.Schematic.Oracle.QueryResult;

internal sealed record GetSynonymNameQueryResult
{
    public string SchemaName { get; init; } = default!;

    public string SynonymName { get; init; } = default!;
}