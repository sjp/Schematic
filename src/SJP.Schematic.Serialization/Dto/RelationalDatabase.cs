using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized database definition.
/// </summary>
public sealed record RelationalDatabase
{
    /// <summary>
    /// The default values applied to the database's identifiers when they are not otherwise qualified.
    /// </summary>
    public required IdentifierDefaults IdentifierDefaults { get; init; }

    /// <summary>
    /// The schemas in the database.
    /// </summary>
    /// <remarks>
    /// Not required, so that a document written before schemas were exported still reads back.
    /// </remarks>
    public IEnumerable<DatabaseSchema> Schemas { get; init; } = [];

    /// <summary>
    /// The tables in the database.
    /// </summary>
    public required IEnumerable<RelationalDatabaseTable> Tables { get; init; }

    /// <summary>
    /// The views in the database.
    /// </summary>
    public required IEnumerable<DatabaseView> Views { get; init; }

    /// <summary>
    /// The sequences in the database.
    /// </summary>
    public required IEnumerable<DatabaseSequence> Sequences { get; init; }

    /// <summary>
    /// The synonyms in the database.
    /// </summary>
    public required IEnumerable<DatabaseSynonym> Synonyms { get; init; }

    /// <summary>
    /// The routines in the database.
    /// </summary>
    public required IEnumerable<DatabaseRoutine> Routines { get; init; }

    /// <summary>
    /// The user-defined types in the database.
    /// </summary>
    /// <remarks>
    /// Not required, so that a document written before user-defined types were exported still reads back.
    /// </remarks>
    public IEnumerable<DatabaseUserDefinedType> UserDefinedTypes { get; init; } = [];
}
