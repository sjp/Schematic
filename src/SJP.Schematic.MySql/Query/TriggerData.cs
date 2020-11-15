namespace SJP.Schematic.MySql.Query
{
    internal sealed record TriggerData
    {
        public string TriggerName { get; init; } = default!;

        public string Definition { get; init; } = default!;

        public string Timing { get; init; } = default!;

        public string TriggerEvent { get; init; } = default!;
    }
}
