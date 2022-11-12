namespace SJP.Schematic.Serialization.Dto;

public class DatabaseTrigger
{
    public Identifier? TriggerName { get; init; }

    public string? Definition { get; init; }

    public required Core.TriggerQueryTiming QueryTiming { get; init; }

    public required Core.TriggerEvent TriggerEvent { get; init; }

    public required bool IsEnabled { get; init; }
}