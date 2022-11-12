namespace SJP.Schematic.Serialization.Dto.Comments;

public class DatabaseSequenceComments
{
    public required Identifier SequenceName { get; init; }

    public string? Comment { get; init; }
}