namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record TriggerData
    {
        public string TriggerName { get; init; } = default!;

        public string Definition { get; init; } = default!;

        public bool IsInsteadOfTrigger { get; init; }

        public string TriggerEvent { get; init; } = default!;

        public bool IsDisabled { get; init; }
    }
}
