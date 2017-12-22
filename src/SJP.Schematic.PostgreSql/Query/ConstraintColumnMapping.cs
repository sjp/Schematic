namespace SJP.Schematic.PostgreSql.Query
{
    public class ConstraintColumnMapping
    {
        public string ConstraintName { get; set; }

        public string ColumnName { get; set; }

        public int OrdinalPosition { get; set; }
    }
}
