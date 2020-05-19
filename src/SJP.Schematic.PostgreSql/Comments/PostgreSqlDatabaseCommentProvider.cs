using System;
using System.Collections.Generic;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.PostgreSql.Comments
{
    public class PostgreSqlDatabaseCommentProvider : IRelationalDatabaseCommentProvider
    {
        public PostgreSqlDatabaseCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (identifierDefaults == null)
                throw new ArgumentNullException(nameof(identifierDefaults));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            _tableCommentProvider = new PostgreSqlTableCommentProvider(connection, identifierDefaults, identifierResolver);
            _viewCommentProvider = new PostgreSqlViewCommentProvider(connection, identifierDefaults, identifierResolver);
            _sequenceCommentProvider = new PostgreSqlSequenceCommentProvider(connection, identifierDefaults, identifierResolver);
            _routineCommentProvider = new PostgreSqlRoutineCommentProvider(connection, identifierDefaults, identifierResolver);
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
        /// Retrieves all database view comments defined within a database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of view comments.</returns>
        public IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments(CancellationToken cancellationToken = default)
        {
            return _viewCommentProvider.GetAllViewComments(cancellationToken);
        }

        /// <summary>
        /// Retrieves comments for a particular database sequence.
        /// </summary>
        /// <param name="sequenceName">The name of a database sequence.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="OptionAsync{A}" /> instance which holds the value of the sequence's comments, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _sequenceCommentProvider.GetSequenceComments(sequenceName, cancellationToken);
        }

        /// <summary>
        /// Retrieves comments for all database sequences.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of database sequence comments.</returns>
        public IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments(CancellationToken cancellationToken = default)
        {
            return _sequenceCommentProvider.GetAllSequenceComments(cancellationToken);
        }

        /// <summary>
        /// Retrieves comments for a database synonym.
        /// </summary>
        /// <param name="synonymName">A synonym name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A comments object result in the some state, if found, none otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
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

        /// <summary>
        /// Retrieves comments for a database routine, if available.
        /// </summary>
        /// <param name="routineName">A routine name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Comments for the given database routine, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return _routineCommentProvider.GetRoutineComments(routineName, cancellationToken);
        }

        /// <summary>
        /// Retrieves all database routine comments defined within a database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of routine comments.</returns>
        public IAsyncEnumerable<IDatabaseRoutineComments> GetAllRoutineComments(CancellationToken cancellationToken = default)
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
