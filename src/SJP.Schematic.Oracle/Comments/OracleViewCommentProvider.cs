using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Oracle.Comments
{
    public class OracleViewCommentProvider : IDatabaseViewCommentProvider
    {
        public OracleViewCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (identifierDefaults == null)
                throw new ArgumentNullException(nameof(identifierDefaults));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            QueryViewCommentProvider = new OracleQueryViewCommentProvider(connection, identifierDefaults, identifierResolver);
            MaterializedViewCommentProvider = new OracleMaterializedViewCommentProvider(connection, identifierDefaults, identifierResolver);
        }

        protected IDatabaseViewCommentProvider QueryViewCommentProvider { get; }

        protected IDatabaseViewCommentProvider MaterializedViewCommentProvider { get; }

        /// <summary>
        /// Retrieves all database view comments defined within a database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of view comments.</returns>
        public async IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryViewCommentsTask = QueryViewCommentProvider.GetAllViewComments(cancellationToken).ToListAsync(cancellationToken).AsTask();
            var materializedViewCommentsTask = MaterializedViewCommentProvider.GetAllViewComments(cancellationToken).ToListAsync(cancellationToken).AsTask();

            await Task.WhenAll(queryViewCommentsTask, materializedViewCommentsTask).ConfigureAwait(false);

            var queryViewComments = await queryViewCommentsTask.ConfigureAwait(false);
            var materializedViewComments = await materializedViewCommentsTask.ConfigureAwait(false);

            var comments = queryViewComments
                .Concat(materializedViewComments)
                .OrderBy(v => v.ViewName.Schema)
                .ThenBy(v => v.ViewName.LocalName);

            foreach (var comment in comments)
                yield return comment;
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

            return QueryViewCommentProvider.GetViewComments(viewName, cancellationToken)
                 | MaterializedViewCommentProvider.GetViewComments(viewName, cancellationToken);
        }
    }
}
