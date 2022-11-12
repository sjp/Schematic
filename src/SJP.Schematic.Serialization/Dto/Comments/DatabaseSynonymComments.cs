namespace SJP.Schematic.Serialization.Dto.Comments;

public class DatabaseSynonymComments
{
    public required Identifier SynonymName { get; init; }

    public string? Comment { get; init; }
}