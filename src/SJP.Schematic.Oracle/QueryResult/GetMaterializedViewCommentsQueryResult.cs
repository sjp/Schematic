namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetMaterializedViewCommentsQueryResult
    {
        public string ColumnName { get; init; } = default!;

        public string ObjectType { get; init; } = default!;

        public string? Comment { get; init; }
    }
}
