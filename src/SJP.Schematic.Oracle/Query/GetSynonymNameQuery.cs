namespace SJP.Schematic.Oracle.Query
{
    internal sealed record GetSynonymNameQuery
    {
        public string SchemaName { get; init; } = default!;

        public string SynonymName { get; init; } = default!;
    }
}
