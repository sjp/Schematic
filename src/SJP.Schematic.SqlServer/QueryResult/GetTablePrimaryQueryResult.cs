namespace SJP.Schematic.SqlServer.QueryResult
{
    internal sealed record GetTablePrimaryQueryResult
    {
        public string ConstraintName { get; init; } = default!;

        public string ColumnName { get; init; } = default!;

        public bool IsDisabled { get; init; }
    }
}
