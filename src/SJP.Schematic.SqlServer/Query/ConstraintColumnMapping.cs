namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record ConstraintColumnMapping
    {
        public string ConstraintName { get; init; } = default!;

        public string ColumnName { get; init; } = default!;

        public bool IsDisabled { get; init; }
    }
}
