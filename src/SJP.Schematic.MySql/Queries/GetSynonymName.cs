namespace SJP.Schematic.MySql.Queries;

internal static class GetSynonymName
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string SynonymName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string SynonymName { get; init; } = default!;
    }
}