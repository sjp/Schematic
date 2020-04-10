namespace SJP.Schematic.Serialization.Dto
{
    public class DatabaseTrigger
    {
        public Identifier? TriggerName { get; set; }

        public string? Definition { get; set; }

        public Core.TriggerQueryTiming QueryTiming { get; set; }

        public Core.TriggerEvent TriggerEvent { get; set; }

        public bool IsEnabled { get; set; }
    }
}