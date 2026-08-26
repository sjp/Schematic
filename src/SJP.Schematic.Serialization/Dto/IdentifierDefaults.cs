namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// The serialized default values applied to a database's identifiers when they are not otherwise qualified.
/// </summary>
public class IdentifierDefaults
{
    /// <summary>
    /// The default server name.
    /// </summary>
    public string? Server { get; init; }

    /// <summary>
    /// The default database name.
    /// </summary>
    public string? Database { get; init; }

    /// <summary>
    /// The default schema name.
    /// </summary>
    public string? Schema { get; init; }
}
