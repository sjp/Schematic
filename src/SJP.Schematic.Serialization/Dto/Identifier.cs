namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized database object name, qualified by as much of a server, database and schema as is known.
/// </summary>
public sealed record Identifier
{
    /// <summary>
    /// The server the object belongs to, if known.
    /// </summary>
    public string? Server { get; init; }

    /// <summary>
    /// The database the object belongs to, if known.
    /// </summary>
    public string? Database { get; init; }

    /// <summary>
    /// The schema the object belongs to, if known.
    /// </summary>
    public string? Schema { get; init; }

    /// <summary>
    /// The name of the object within its schema.
    /// </summary>
    public required string LocalName { get; init; }
}
