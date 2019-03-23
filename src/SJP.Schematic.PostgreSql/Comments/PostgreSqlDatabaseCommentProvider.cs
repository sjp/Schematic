using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.PostgreSql.Comments
{
    public class PostgreSqlDatabaseCommentProvider : IRelationalDatabaseCommentProvider
    {
        public PostgreSqlDatabaseCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (identifierDefaults == null)
                throw new ArgumentNullException(nameof(identifierDefaults));

            _tableCommentProvider = new PostgreSqlTableCommentProvider(connection, identifierDefaults);
            _viewCommentProvider = new PostgreSqlViewCommentProvider(connection, identifierDefaults);
            _sequenceCommentProvider = new PostgreSqlSequenceCommentProvider(connection, identifierDefaults);
            _routineCommentProvider = new PostgreSqlRoutineCommentProvider(connection, identifierDefaults);
        }

        public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return _tableCommentProvider.GetTableComments(tableName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> GetAllTableComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _tableCommentProvider.GetAllTableComments(cancellationToken);
        }

        public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return _viewCommentProvider.GetViewComments(viewName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _viewCommentProvider.GetAllViewComments(cancellationToken);
        }

        public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _sequenceCommentProvider.GetSequenceComments(sequenceName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseSequenceComments>> GetAllSequenceComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _sequenceCommentProvider.GetAllSequenceComments(cancellationToken);
        }

        public OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return SynonymCommentProvider.GetSynonymComments(synonymName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseSynonymComments>> GetAllSynonymComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SynonymCommentProvider.GetAllSynonymComments(cancellationToken);
        }

        public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return _routineCommentProvider.GetRoutineComments(routineName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseRoutineComments>> GetAllRoutineComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _routineCommentProvider.GetAllRoutineComments(cancellationToken);
        }

        private readonly IRelationalDatabaseTableCommentProvider _tableCommentProvider;
        private readonly IDatabaseViewCommentProvider _viewCommentProvider;
        private readonly IDatabaseSequenceCommentProvider _sequenceCommentProvider;
        private readonly IDatabaseRoutineCommentProvider _routineCommentProvider;

        private static readonly IDatabaseSynonymCommentProvider SynonymCommentProvider = new EmptyDatabaseSynonymCommentProvider();
    }
}
