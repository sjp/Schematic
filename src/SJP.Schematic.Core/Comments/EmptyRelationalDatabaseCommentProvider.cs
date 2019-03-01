using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public sealed class EmptyRelationalDatabaseCommentProvider : IRelationalDatabaseCommentProvider
    {
        public Task<IReadOnlyCollection<IDatabaseRoutineComments>> GetAllRoutineComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _routineCommentProvider.GetAllRoutineComments(cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseSequenceComments>> GetAllSequenceComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _sequenceCommentProvider.GetAllSequenceComments(cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseSynonymComments>> GetAllSynonymComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _synonymCommentProvider.GetAllSynonymComments(cancellationToken);
        }

        public Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> GetAllTableComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _tableCommentProvider.GetAllTableComments(cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _viewCommentProvider.GetAllViewComments(cancellationToken);
        }

        public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return _routineCommentProvider.GetRoutineComments(routineName, cancellationToken);
        }

        public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _sequenceCommentProvider.GetSequenceComments(sequenceName, cancellationToken);
        }

        public OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return _synonymCommentProvider.GetSynonymComments(synonymName, cancellationToken);
        }

        public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return _tableCommentProvider.GetTableComments(tableName, cancellationToken);
        }

        public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
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
