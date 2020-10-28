namespace SJP.Schematic.MySql.Query
{
    internal sealed class TriggerData
    {
        public string TriggerName { get; set; } = default!;

        public string Definition { get; set; } = default!;

        public string Timing { get; set; } = default!;

        public string TriggerEvent { get; set; } = default!;
    }
}
