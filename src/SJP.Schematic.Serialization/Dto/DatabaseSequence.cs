namespace SJP.Schematic.Serialization.Dto;

public class DatabaseSequence
{
    public Identifier? SequenceName { get; init; }

    public required int Cache { get; init; }

    public required bool Cycle { get; init; }

    public required decimal Increment { get; init; }

    public decimal? MaxValue { get; init; }

    public decimal? MinValue { get; init; }

    public required decimal Start { get; init; }
}