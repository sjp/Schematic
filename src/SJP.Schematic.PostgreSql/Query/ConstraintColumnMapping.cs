namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed record ConstraintColumnMapping
    {
        public string ConstraintName { get; init; } = default!;

        public string ColumnName { get; init; } = default!;

        public int OrdinalPosition { get; init; }
    }
}
