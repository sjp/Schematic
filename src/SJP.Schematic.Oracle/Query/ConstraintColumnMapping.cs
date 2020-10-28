namespace SJP.Schematic.Oracle.Query
{
    internal sealed class ConstraintColumnMapping
    {
        public string? ConstraintName { get; set; }

        public string? EnabledStatus { get; set; }

        public string? ColumnName { get; set; }

        public int ColumnPosition { get; set; }
    }
}
