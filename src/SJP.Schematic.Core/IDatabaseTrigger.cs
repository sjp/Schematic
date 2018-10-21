namespace SJP.Schematic.Core
{
    public interface IDatabaseTrigger : IDatabaseEntity, IDatabaseOptional
    {
        string Definition { get; }

        TriggerQueryTiming QueryTiming { get; }

        TriggerEvent TriggerEvent { get; }
    }
}
