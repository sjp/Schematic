namespace SJP.Schematic.SqlServer.Query
{
    internal sealed class CheckConstraintData
    {
        public string ConstraintName { get; set; } = default!;

        public string Definition { get; set; } = default!;

        public bool IsDisabled { get; set; }
    }
}
