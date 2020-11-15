namespace SJP.Schematic.MySql.Query
{
    internal sealed record ConstraintColumnMapping
    {
        public string ConstraintName { get; init; } = default!;

        public string ColumnName { get; init; } = default!;
    }
}
