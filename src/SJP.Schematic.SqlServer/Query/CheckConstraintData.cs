namespace SJP.Schematic.SqlServer.Query
{
    internal class CheckConstraintData
    {
        public string ConstraintName { get; set; } = default!;

        public string Definition { get; set; } = default!;

        public bool IsDisabled { get; set; }
    }
}
