namespace SJP.Schematic.SqlServer.QueryResult
{
    internal sealed record GetAllSequenceCommentsQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string SequenceName { get; init; } = default!;

        public string ObjectType { get; init; } = default!;

        public string ObjectName { get; init; } = default!;

        public string? Comment { get; init; }
    }
}
