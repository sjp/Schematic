namespace SJP.Schematic.Reporting.Snapshot.Query
{
    internal record AddDatabaseCommentQuery
    {
        public string ObjectType { get; init; } = default!;

        public string? ServerName { get; init; }

        public string? DatabaseName { get; init; }

        public string? SchemaName { get; init; }

        public string? LocalName { get; init; } = default!;

        public string CommentJson { get; init; } = default!;
    }
}
