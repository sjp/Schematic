namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized database synonym, i.e. an alias for another object.
/// </summary>
public class DatabaseSynonym
{
    /// <summary>
    /// The name of the synonym.
    /// </summary>
    public required Identifier SynonymName { get; init; }

    /// <summary>
    /// The name of the object being aliased. The target need not be present in the database.
    /// </summary>
    public required Identifier Target { get; init; }
}
