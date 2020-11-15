namespace SJP.Schematic.Oracle.Query
{
    internal sealed record TriggerData
    {
        public string? TriggerSchema { get; init; }

        public string? TriggerName { get; init; }

        public string? TriggerType { get; init; }

        public string? TriggerEvent { get; init; }

        public string? Definition { get; init; }

        public string? EnabledStatus { get; init; }
    }
}
