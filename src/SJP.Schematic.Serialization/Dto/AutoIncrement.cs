namespace SJP.Schematic.Serialization.Dto;

public class AutoIncrement
{
    public required decimal InitialValue { get; init; }

    public required decimal Increment { get; init; }
}