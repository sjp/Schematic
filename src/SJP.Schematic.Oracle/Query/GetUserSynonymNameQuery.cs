namespace SJP.Schematic.Oracle.Query
{
    internal sealed record GetUserSynonymNameQuery
    {
        public string SynonymName { get; init; } = default!;
    }
}
