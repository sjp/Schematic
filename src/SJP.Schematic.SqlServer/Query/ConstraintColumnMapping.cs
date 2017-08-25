namespace SJP.Schematic.SqlServer.Query
{
    public class ConstraintColumnMapping
    {
        public string ConstraintName { get; set; }

        public string ColumnName { get; set; }

        public bool IsDisabled { get; set; }
    }
}
