namespace SJP.Schematic.SqlServer.Query
{
    internal class ConstraintColumnMapping
    {
        public string ConstraintName { get; set; } = default!;

        public string ColumnName { get; set; } = default!;

        public bool IsDisabled { get; set; }
    }
}
