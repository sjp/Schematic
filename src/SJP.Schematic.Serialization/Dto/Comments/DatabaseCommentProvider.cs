using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto.Comments;

public class DatabaseCommentProvider
{
    public required IdentifierDefaults IdentifierDefaults { get; init; }

    public required IEnumerable<DatabaseTableComments> TableComments { get; init; }

    public required IEnumerable<DatabaseViewComments> ViewComments { get; init; }

    public required IEnumerable<DatabaseSequenceComments> SequenceComments { get; init; }

    public required IEnumerable<DatabaseSynonymComments> SynonymComments { get; init; }

    public required IEnumerable<DatabaseRoutineComments> RoutineComments { get; init; }
}