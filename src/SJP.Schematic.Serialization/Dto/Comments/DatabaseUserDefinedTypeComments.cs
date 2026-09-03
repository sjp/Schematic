namespace SJP.Schematic.Serialization.Dto.Comments;

/// <summary>
/// The serialized comments attached to a database user-defined type.
/// </summary>
public sealed record DatabaseUserDefinedTypeComments
{
    /// <summary>
    /// The name of the type the comments are attached to.
    /// </summary>
    public required Identifier TypeName { get; init; }

    /// <summary>
    /// The comment attached to the type, if any.
    /// </summary>
    public string? Comment { get; init; }
}
