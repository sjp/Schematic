namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetTableIndexesQueryResult
    {
        public string? IndexOwner { get; init; }

        public string? IndexName { get; init; }

        public string? Uniqueness { get; init; }

        public string? IsDescending { get; init; }

        public string? ColumnName { get; init; }

        public int ColumnPosition { get; init; }
    }
}
