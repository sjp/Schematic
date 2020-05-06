namespace SJP.Schematic.Core.Comments
{
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
    }
}
