namespace SJP.Schematic.SqlServer.Query
{
    internal class CheckConstraintData
    {
        public string? ConstraintName { get; set; }

        public string? Definition { get; set; }

        public bool IsDisabled { get; set; }
    }
}
