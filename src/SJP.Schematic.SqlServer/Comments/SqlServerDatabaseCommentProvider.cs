using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.SqlServer.Comments
{
    public class SqlServerDatabaseCommentProvider : IRelationalDatabaseCommentProvider
    {
        public SqlServerDatabaseCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (identifierDefaults == null)
                throw new ArgumentNullException(nameof(identifierDefaults));

            _tableCommentProvider = new SqlServerTableCommentProvider(connection, identifierDefaults);
            _viewCommentProvider = new SqlServerViewCommentProvider(connection, identifierDefaults);
            _sequenceCommentProvider = new SqlServerSequenceCommentProvider(connection, identifierDefaults);
            _synonymCommentProvider = new SqlServerSynonymCommentProvider(connection, identifierDefaults);
            _routineCommentProvider = new SqlServerRoutineCommentProvider(connection, identifierDefaults);
        }

        public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return _tableCommentProvider.GetTableComments(tableName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> GetAllTableComments(CancellationToken cancellationToken = default)
        {
            return _tableCommentProvider.GetAllTableComments(cancellationToken);
        }

        public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return _viewCommentProvider.GetViewComments(viewName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments(CancellationToken cancellationToken = default)
        {
            return _viewCommentProvider.GetAllViewComments(cancellationToken);
        }

        public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _sequenceCommentProvider.GetSequenceComments(sequenceName, cancellationToken);
        }

        public IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments(CancellationToken cancellationToken = default)
        {
            return _sequenceCommentProvider.GetAllSequenceComments(cancellationToken);
        }

        public OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return _synonymCommentProvider.GetSynonymComments(synonymName, cancellationToken);
        }

        public IAsyncEnumerable<IDatabaseSynonymComments> GetAllSynonymComments(CancellationToken cancellationToken = default)
        {
            return _synonymCommentProvider.GetAllSynonymComments(cancellationToken);
        }

        public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return _routineCommentProvider.GetRoutineComments(routineName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseRoutineComments>> GetAllRoutineComments(CancellationToken cancellationToken = default)
        {
            return _routineCommentProvider.GetAllRoutineComments(cancellationToken);
        }

        private readonly IRelationalDatabaseTableCommentProvider _tableCommentProvider;
        private readonly IDatabaseViewCommentProvider _viewCommentProvider;
        private readonly IDatabaseSequenceCommentProvider _sequenceCommentProvider;
        private readonly IDatabaseSynonymCommentProvider _synonymCommentProvider;
        private readonly IDatabaseRoutineCommentProvider _routineCommentProvider;
    }
}
