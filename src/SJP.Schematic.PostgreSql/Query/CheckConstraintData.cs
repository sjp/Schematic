namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed class CheckConstraintData
    {
        public string? ConstraintName { get; set; }

        public string? Definition { get; set; }
    }
}
