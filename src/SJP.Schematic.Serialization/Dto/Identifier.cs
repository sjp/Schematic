namespace SJP.Schematic.Serialization.Dto;

public class Identifier
{
    public string? Server { get; init; }

    public string? Database { get; init; }

    public string? Schema { get; init; }

    public string? LocalName { get; init; }
}