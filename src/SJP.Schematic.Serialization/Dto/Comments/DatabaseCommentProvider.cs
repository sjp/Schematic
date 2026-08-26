using System.Collections.Generic;
using System.Text.Json.Serialization;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Dto.Comments;

public class DatabaseCommentProvider
{
    public required IdentifierDefaults IdentifierDefaults { get; init; }

    /// <summary>
    /// Runtime-only state used to pass the caller's resolver to the mapper. Never (de)serialized.
    /// </summary>
    [JsonIgnore]
    public IIdentifierResolutionStrategy? IdentifierResolver { get; set; }

    public required IEnumerable<DatabaseTableComments> TableComments { get; init; }

    public required IEnumerable<DatabaseViewComments> ViewComments { get; init; }

    public required IEnumerable<DatabaseSequenceComments> SequenceComments { get; init; }

    public required IEnumerable<DatabaseSynonymComments> SynonymComments { get; init; }

    public required IEnumerable<DatabaseRoutineComments> RoutineComments { get; init; }
}