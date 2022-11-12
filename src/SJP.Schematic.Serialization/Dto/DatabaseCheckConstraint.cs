namespace SJP.Schematic.Serialization.Dto;

public class DatabaseCheckConstraint
{
    public Identifier? CheckName { get; init; }

    public string? Definition { get; init; }

    public required bool IsEnabled { get; init; }
}