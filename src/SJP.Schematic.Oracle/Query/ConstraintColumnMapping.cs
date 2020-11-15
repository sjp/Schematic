namespace SJP.Schematic.Oracle.Query
{
    internal sealed record ConstraintColumnMapping
    {
        public string? ConstraintName { get; init; }

        public string? EnabledStatus { get; init; }

        public string? ColumnName { get; init; }

        public int ColumnPosition { get; init; }
    }
}
