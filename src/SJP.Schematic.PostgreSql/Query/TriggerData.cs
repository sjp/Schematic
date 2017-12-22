namespace SJP.Schematic.PostgreSql.Query
{
    public class TriggerData
    {
        public string TriggerName { get; set; }

        public string Definition { get; set; }

        public string Timing { get; set; }

        public string TriggerEvent { get; set; }

        public string EnabledFlag { get; set; }
    }
}
