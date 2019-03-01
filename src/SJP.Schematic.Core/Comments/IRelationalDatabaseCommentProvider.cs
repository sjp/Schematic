namespace SJP.Schematic.Core.Comments
{
    public interface IRelationalDatabaseCommentProvider
        : IRelationalDatabaseTableCommentProvider,
          IDatabaseViewCommentProvider,
          IDatabaseSequenceCommentProvider,
          IDatabaseSynonymCommentProvider,
          IDatabaseRoutineCommentProvider
    {
    }
}
