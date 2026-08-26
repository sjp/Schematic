namespace SJP.Schematic.Serialization.Dto.Comments;

/// <summary>
/// The serialized comments attached to a database synonym.
/// </summary>
public class DatabaseSynonymComments
{
    /// <summary>
    /// The name of the synonym the comments are attached to.
    /// </summary>
    public required Identifier SynonymName { get; init; }

    /// <summary>
    /// The comment attached to the synonym, if any.
    /// </summary>
    public string? Comment { get; init; }
}
