namespace SJP.Schematic.MySql.Query
{
    internal sealed class ConstraintColumnMapping
    {
        public string ConstraintName { get; set; } = default!;

        public string ColumnName { get; set; } = default!;
    }
}
