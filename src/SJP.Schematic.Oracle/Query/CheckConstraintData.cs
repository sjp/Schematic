namespace SJP.Schematic.Oracle.Query
{
    internal sealed record CheckConstraintData
    {
        public string ConstraintName { get; init; } = default!;

        public string? Definition { get; init; }

        public string EnabledStatus { get; init; } = default!;
    }
}
