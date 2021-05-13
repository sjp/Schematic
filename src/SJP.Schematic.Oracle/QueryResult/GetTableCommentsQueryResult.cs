namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetTableCommentsQueryResult
    {
        public string ColumnName { get; init; } = default!;

        public string ObjectType { get; init; } = default!;

        public string? Comment { get; init; }
    }
}
