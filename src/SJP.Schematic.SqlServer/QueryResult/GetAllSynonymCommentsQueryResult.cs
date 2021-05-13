namespace SJP.Schematic.SqlServer.QueryResult
{
    internal sealed record GetAllSynonymCommentsQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string SynonymName { get; init; } = default!;

        public string ObjectType { get; init; } = default!;

        public string ObjectName { get; init; } = default!;

        public string? Comment { get; init; }
    }
}
