namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized database schema, i.e. the namespace that database objects are declared within.
/// </summary>
public sealed record DatabaseSchema
{
    /// <summary>
    /// The name of the schema.
    /// </summary>
    public required Identifier SchemaName { get; init; }

    /// <summary>
    /// The principal that owns the schema, if the source database records one.
    /// </summary>
    public string? Owner { get; init; }

    /// <summary>
    /// Whether this is the schema that unqualified object names resolve to.
    /// </summary>
    public bool IsDefault { get; init; }

    /// <summary>
    /// Whether the schema is declared by the database rather than by a user.
    /// </summary>
    public bool IsSystem { get; init; }
}
