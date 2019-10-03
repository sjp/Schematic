using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Oracle.Comments
{
    public class OracleDatabaseCommentProvider : IRelationalDatabaseCommentProvider
    {
        public OracleDatabaseCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (identifierDefaults == null)
                throw new ArgumentNullException(nameof(identifierDefaults));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            _tableCommentProvider = new OracleTableCommentProvider(connection, identifierDefaults, identifierResolver);
            _viewCommentProvider = new OracleViewCommentProvider(connection, identifierDefaults, identifierResolver);
        }

        public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return _tableCommentProvider.GetTableComments(tableName, cancellationToken);
        }

        public IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments(CancellationToken cancellationToken = default)
        {
            return _tableCommentProvider.GetAllTableComments(cancellationToken);
        }

        public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return _viewCommentProvider.GetViewComments(viewName, cancellationToken);
        }

        public IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments(CancellationToken cancellationToken = default)
        {
            return _viewCommentProvider.GetAllViewComments(cancellationToken);
        }

        public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return SequenceCommentProvider.GetSequenceComments(sequenceName, cancellationToken);
        }

        public IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments(CancellationToken cancellationToken = default)
        {
            return SequenceCommentProvider.GetAllSequenceComments(cancellationToken);
        }

        public OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return SynonymCommentProvider.GetSynonymComments(synonymName, cancellationToken);
        }

        public IAsyncEnumerable<IDatabaseSynonymComments> GetAllSynonymComments(CancellationToken cancellationToken = default)
        {
            return SynonymCommentProvider.GetAllSynonymComments(cancellationToken);
        }

        public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return RoutineCommentProvider.GetRoutineComments(routineName, cancellationToken);
        }

        public IAsyncEnumerable<IDatabaseRoutineComments> GetAllRoutineComments(CancellationToken cancellationToken = default)
        {
            return RoutineCommentProvider.GetAllRoutineComments(cancellationToken);
        }

        private readonly IRelationalDatabaseTableCommentProvider _tableCommentProvider;
        private readonly IDatabaseViewCommentProvider _viewCommentProvider;

        private static readonly IDatabaseSequenceCommentProvider SequenceCommentProvider = new EmptyDatabaseSequenceCommentProvider();
        private static readonly IDatabaseSynonymCommentProvider SynonymCommentProvider = new EmptyDatabaseSynonymCommentProvider();
        private static readonly IDatabaseRoutineCommentProvider RoutineCommentProvider = new EmptyDatabaseRoutineCommentProvider();
    }
}
