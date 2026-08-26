namespace SJP.Schematic.Serialization.Dto.Comments;

/// <summary>
/// The serialized comments attached to a database sequence.
/// </summary>
public class DatabaseSequenceComments
{
    /// <summary>
    /// The name of the sequence the comments are attached to.
    /// </summary>
    public required Identifier SequenceName { get; init; }

    /// <summary>
    /// The comment attached to the sequence, if any.
    /// </summary>
    public string? Comment { get; init; }
}
