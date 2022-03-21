namespace SJP.Schematic.Serialization.Dto.Comments;

public class DatabaseSynonymComments
{
    public Identifier SynonymName { get; set; } = default!;

    public string? Comment { get; set; }
}