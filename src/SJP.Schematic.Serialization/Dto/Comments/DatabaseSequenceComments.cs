namespace SJP.Schematic.Serialization.Dto.Comments;

public class DatabaseSequenceComments
{
    public Identifier SequenceName { get; set; } = default!;

    public string? Comment { get; set; }
}