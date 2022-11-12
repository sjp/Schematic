namespace SJP.Schematic.Serialization.Dto;

public class DatabaseSynonym
{
    public Identifier? SynonymName { get; init; }

    public Identifier? Target { get; init; }
}