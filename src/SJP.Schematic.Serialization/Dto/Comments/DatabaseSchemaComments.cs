namespace SJP.Schematic.Serialization.Dto.Comments;

/// <summary>
/// The serialized comments attached to a database schema.
/// </summary>
public sealed record DatabaseSchemaComments
{
    /// <summary>
    /// The name of the schema the comments are attached to.
    /// </summary>
    public required Identifier SchemaName { get; init; }

    /// <summary>
    /// The comment attached to the schema, if any.
    /// </summary>
    public string? Comment { get; init; }
}
