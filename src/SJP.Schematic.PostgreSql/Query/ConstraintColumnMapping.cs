namespace SJP.Schematic.PostgreSql.Query
{
    internal class ConstraintColumnMapping
    {
        public string ConstraintName { get; set; } = default!;

        public string ColumnName { get; set; } = default!;

        public int OrdinalPosition { get; set; }
    }
}
