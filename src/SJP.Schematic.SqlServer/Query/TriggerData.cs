namespace SJP.Schematic.SqlServer.Query
{
    internal class TriggerData
    {
        public string TriggerName { get; set; } = default!;

        public string Definition { get; set; } = default!;

        public bool IsInsteadOfTrigger { get; set; }

        public string TriggerEvent { get; set; } = default!;

        public bool IsDisabled { get; set; }
    }
}
