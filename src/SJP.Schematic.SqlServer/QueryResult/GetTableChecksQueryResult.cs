namespace SJP.Schematic.SqlServer.QueryResult
{
    internal sealed record GetTableChecksQueryResult
    {
        public string ConstraintName { get; init; } = default!;

        public string Definition { get; init; } = default!;

        public bool IsDisabled { get; init; }
    }
}
