using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Dto.Comments;

public class DatabaseCommentProvider
{
    public IdentifierDefaults IdentifierDefaults { get; set; } = default!;

    public IIdentifierResolutionStrategy? IdentifierResolver { get; set; }

    public required IEnumerable<DatabaseTableComments> TableComments { get; init; }

    public required IEnumerable<DatabaseViewComments> ViewComments { get; init; }

    public required IEnumerable<DatabaseSequenceComments> SequenceComments { get; init; }

    public required IEnumerable<DatabaseSynonymComments> SynonymComments { get; init; }

    public required IEnumerable<DatabaseRoutineComments> RoutineComments { get; init; }
}