using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto.Comments;

/// <summary>
/// The serialized comments attached to a database's objects.
/// </summary>
public class DatabaseCommentProvider
{
    /// <summary>
    /// The default values applied to the database's identifiers when they are not otherwise qualified.
    /// </summary>
    public required IdentifierDefaults IdentifierDefaults { get; init; }

    /// <summary>
    /// The comments attached to the database's tables.
    /// </summary>
    public required IEnumerable<DatabaseTableComments> TableComments { get; init; }

    /// <summary>
    /// The comments attached to the database's views.
    /// </summary>
    public required IEnumerable<DatabaseViewComments> ViewComments { get; init; }

    /// <summary>
    /// The comments attached to the database's sequences.
    /// </summary>
    public required IEnumerable<DatabaseSequenceComments> SequenceComments { get; init; }

    /// <summary>
    /// The comments attached to the database's synonyms.
    /// </summary>
    public required IEnumerable<DatabaseSynonymComments> SynonymComments { get; init; }

    /// <summary>
    /// The comments attached to the database's routines.
    /// </summary>
    public required IEnumerable<DatabaseRoutineComments> RoutineComments { get; init; }
}
