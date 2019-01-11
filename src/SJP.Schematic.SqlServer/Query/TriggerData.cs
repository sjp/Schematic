namespace SJP.Schematic.SqlServer.Query
{
    internal class TriggerData
    {
        public string TriggerName { get; set; }

        public string Definition { get; set; }

        public bool IsInsteadOfTrigger { get; set; }

        public string TriggerEvent { get; set; }

        public bool IsDisabled { get; set; }
    }
}
