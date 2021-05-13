namespace SJP.Schematic.PostgreSql.QueryResult
{
    internal sealed record GetAllSequenceCommentsQueryResult
    {
        public string? SchemaName { get; init; }

        public string? SequenceName { get; init; }

        public string? Comment { get; init; }
    }
}
