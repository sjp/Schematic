namespace SJP.Schematic.Oracle.Query;

internal sealed record GetUserSynonymDefinitionQuery
{
    public string SynonymName { get; init; } = default!;
}
