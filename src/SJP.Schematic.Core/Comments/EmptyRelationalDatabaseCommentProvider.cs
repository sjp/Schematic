using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public sealed class EmptyRelationalDatabaseCommentProvider : IRelationalDatabaseCommentProvider
    {
        public Task<IReadOnlyCollection<IDatabaseRoutineComments>> GetAllRoutineComments(CancellationToken cancellationToken = default)
        {
            return _routineCommentProvider.GetAllRoutineComments(cancellationToken);
        }

        public IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments(CancellationToken cancellationToken = default)
        {
            return _sequenceCommentProvider.GetAllSequenceComments(cancellationToken);
        }

        public IAsyncEnumerable<IDatabaseSynonymComments> GetAllSynonymComments(CancellationToken cancellationToken = default)
        {
            return _synonymCommentProvider.GetAllSynonymComments(cancellationToken);
        }

        public IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments(CancellationToken cancellationToken = default)
        {
            return _tableCommentProvider.GetAllTableComments(cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments(CancellationToken cancellationToken = default)
        {
            return _viewCommentProvider.GetAllViewComments(cancellationToken);
        }

        public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return _routineCommentProvider.GetRoutineComments(routineName, cancellationToken);
        }

        public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _sequenceCommentProvider.GetSequenceComments(sequenceName, cancellationToken);
        }

        public OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return _synonymCommentProvider.GetSynonymComments(synonymName, cancellationToken);
        }

        public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return _tableCommentProvider.GetTableComments(tableName, cancellationToken);
        }

        public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return _viewCommentProvider.GetViewComments(viewName, cancellationToken);
        }

        private static readonly IRelationalDatabaseTableCommentProvider _tableCommentProvider = new EmptyRelationalDatabaseTableCommentProvider();
        private static readonly IDatabaseViewCommentProvider _viewCommentProvider = new EmptyDatabaseViewCommentProvider();
        private static readonly IDatabaseSequenceCommentProvider _sequenceCommentProvider = new EmptyDatabaseSequenceCommentProvider();
        private static readonly IDatabaseSynonymCommentProvider _synonymCommentProvider = new EmptyDatabaseSynonymCommentProvider();
        private static readonly IDatabaseRoutineCommentProvider _routineCommentProvider = new EmptyDatabaseRoutineCommentProvider();
    }
}
