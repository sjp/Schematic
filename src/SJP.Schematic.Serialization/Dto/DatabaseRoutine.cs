namespace SJP.Schematic.Serialization.Dto;

public class DatabaseRoutine
{
    public required Identifier RoutineName { get; init; }

    public required string Definition { get; init; }
}