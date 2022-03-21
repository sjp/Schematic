namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Defines a container type which retrieves comments associated with all top-level database objects.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableCommentProvider" />
/// <seealso cref="IDatabaseViewCommentProvider" />
/// <seealso cref="IDatabaseSequenceCommentProvider" />
/// <seealso cref="IDatabaseSynonymCommentProvider" />
/// <seealso cref="IDatabaseRoutineCommentProvider" />
public interface IRelationalDatabaseCommentProvider
    : IRelationalDatabaseTableCommentProvider,
      IDatabaseViewCommentProvider,
      IDatabaseSequenceCommentProvider,
      IDatabaseSynonymCommentProvider,
      IDatabaseRoutineCommentProvider
{
    /// <summary>
    /// Identifier defaults. Used to determine the default name resolution applied to the database.
    /// </summary>
    /// <value>The identifier defaults.</value>
    IIdentifierDefaults IdentifierDefaults { get; }
}