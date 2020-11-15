namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record CheckConstraintData
    {
        public string ConstraintName { get; init; } = default!;

        public string Definition { get; init; } = default!;

        public bool IsDisabled { get; init; }
    }
}
