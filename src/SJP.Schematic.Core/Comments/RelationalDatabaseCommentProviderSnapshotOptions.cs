namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Defines which database entity comments will be populated when snapshotting a database's comments.
/// </summary>
public sealed record RelationalDatabaseCommentProviderSnapshotOptions
{
    /// <summary>
    /// Indicates whether table comments should be exported. The default is <c>true</c>.
    /// </summary>
    public bool IncludeTableComments { get; init; } = true;

    /// <summary>
    /// Indicates whether view comments should be exported. The default is <c>true</c>.
    /// </summary>
    public bool IncludeViewComments { get; init; } = true;

    /// <summary>
    /// Indicates whether routine comments should be exported. The default is <c>true</c>.
    /// </summary>
    public bool IncludeRoutineComments { get; init; } = true;

    /// <summary>
    /// Indicates whether sequence comments should be exported. The default is <c>true</c>.
    /// </summary>
    public bool IncludeSequenceComments { get; init; } = true;

    /// <summary>
    /// Indicates whether synonym comments should be exported. The default is <c>true</c>.
    /// </summary>
    public bool IncludeSynonymComments { get; init; } = true;

    /// <summary>
    /// Configures options so that no database comments will be exported.
    /// This is intended to be used to enable opt-in approaches, rather than the default of opt-out.
    /// </summary>
    public static RelationalDatabaseCommentProviderSnapshotOptions Empty => new RelationalDatabaseCommentProviderSnapshotOptions
    {
        IncludeTableComments = false,
        IncludeViewComments = false,
        IncludeSequenceComments = false,
        IncludeRoutineComments = false,
        IncludeSynonymComments = false,
    };
}
