namespace SJP.Schematic.Oracle.Query
{
    internal sealed class TriggerData
    {
        public string? TriggerSchema { get; set; }

        public string? TriggerName { get; set; }

        public string? TriggerType { get; set; }

        public string? TriggerEvent { get; set; }

        public string? Definition { get; set; }

        public string? EnabledStatus { get; set; }
    }
}
