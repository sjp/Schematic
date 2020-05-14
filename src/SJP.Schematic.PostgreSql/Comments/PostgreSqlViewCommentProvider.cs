using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.PostgreSql.Comments
{
    public class PostgreSqlViewCommentProvider : IDatabaseViewCommentProvider
    {
        public PostgreSqlViewCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (identifierDefaults == null)
                throw new ArgumentNullException(nameof(identifierDefaults));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            QueryViewCommentProvider = new PostgreSqlQueryViewCommentProvider(connection, identifierDefaults, identifierResolver);
            MaterializedViewCommentProvider = new PostgreSqlMaterializedViewCommentProvider(connection, identifierDefaults, identifierResolver);
        }

        protected IDatabaseViewCommentProvider QueryViewCommentProvider { get; }

        protected IDatabaseViewCommentProvider MaterializedViewCommentProvider { get; }

        public async IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryViewsTask = QueryViewCommentProvider.GetAllViewComments(cancellationToken).ToListAsync(cancellationToken).AsTask();
            var materializedViewsTask = MaterializedViewCommentProvider.GetAllViewComments(cancellationToken).ToListAsync(cancellationToken).AsTask();

            await Task.WhenAll(queryViewsTask, materializedViewsTask).ConfigureAwait(false);

            var queryViews = await queryViewsTask.ConfigureAwait(false);
            var materializedViews = await materializedViewsTask.ConfigureAwait(false);

            var viewComments = queryViews
                .Concat(materializedViews)
                .OrderBy(v => v.ViewName.Schema)
                .ThenBy(v => v.ViewName.LocalName);

            foreach (var comment in viewComments)
                yield return comment;
        }

        public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return QueryViewCommentProvider.GetViewComments(viewName, cancellationToken)
                | MaterializedViewCommentProvider.GetViewComments(viewName, cancellationToken);
        }
    }
}
