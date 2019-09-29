namespace SJP.Schematic.PostgreSql.Query
{
    internal class TriggerData
    {
        public string? TriggerName { get; set; }

        public string? Definition { get; set; }

        public string? Timing { get; set; }

        public string? TriggerEvent { get; set; }

        public string? EnabledFlag { get; set; }
    }
}
