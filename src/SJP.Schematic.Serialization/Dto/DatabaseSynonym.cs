namespace SJP.Schematic.Serialization.Dto;

public class DatabaseSynonym
{
    public required Identifier SynonymName { get; init; }

    public required Identifier Target { get; init; }
}