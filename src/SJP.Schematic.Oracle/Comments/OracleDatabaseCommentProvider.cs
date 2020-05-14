using System;
using System.Collections.Generic;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Oracle.Comments
{
    public class OracleDatabaseCommentProvider : IRelationalDatabaseCommentProvider
    {
        public OracleDatabaseCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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

        /// <summary>
        /// Retrieves comments for a database table, if available.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Comments for the given database table, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return _tableCommentProvider.GetTableComments(tableName, cancellationToken);
        }

        /// <summary>
        /// Retrieves comments for all database tables.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of database table comments, where available.</returns>
        public IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments(CancellationToken cancellationToken = default)
        {
            return _tableCommentProvider.GetAllTableComments(cancellationToken);
        }

        /// <summary>
        /// Retrieves comments for a particular database view.
        /// </summary>
        /// <param name="viewName">The name of a database view.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="T:LanguageExt.OptionAsync`1" /> instance which holds the value of the view's comments, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return _viewCommentProvider.GetViewComments(viewName, cancellationToken);
        }

        /// <summary>
        /// Retrieves comments for all database views.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An collection of database view comments.</returns>
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

        /// <summary>
        /// Retrieves comments for all database sequences.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of database sequence comments.</returns>
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

        /// <summary>
        /// Retrieves comments for all database synonyms.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of database synonyms comments.</returns>
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

        /// <summary>
        /// Retrieves all database routine comments defined within a database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of routine comments.</returns>
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
