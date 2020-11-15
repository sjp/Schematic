namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed record CheckConstraintData
    {
        public string? ConstraintName { get; init; }

        public string? Definition { get; init; }
    }
}
