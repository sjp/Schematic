using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.PostgreSql.Comments
{
    public class PostgreSqlViewCommentProvider : IDatabaseViewCommentProvider
    {
        public PostgreSqlViewCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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

        public async Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments(CancellationToken cancellationToken = default)
        {
            var queryViews = await QueryViewCommentProvider.GetAllViewComments(cancellationToken).ConfigureAwait(false);
            var materializedViews = await MaterializedViewCommentProvider.GetAllViewComments(cancellationToken).ConfigureAwait(false);

            return queryViews
                .Concat(materializedViews)
                .OrderBy(v => v.ViewName.Schema)
                .ThenBy(v => v.ViewName.LocalName)
                .ToList();
        }

        public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return GetViewCommentsAsyncCore(viewName, cancellationToken).ToAsync();
        }

        // This is equivalent to the '|' operator on OptionAsync<T> objects.
        // However, this method is necessary because we need to set ConfigureAwait(false)
        // on the async task, which the built-in '|' operator does not do.
        // Without this method we get InProgressException errors...
        private async Task<Option<IDatabaseViewComments>> GetViewCommentsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var queryViewComments = QueryViewCommentProvider.GetViewComments(viewName, cancellationToken);
            var queryViewIsSome = await queryViewComments.IsSome.ConfigureAwait(false);
            if (queryViewIsSome)
                return await queryViewComments.ToOption().ConfigureAwait(false);

            var materializedViewComments = MaterializedViewCommentProvider.GetViewComments(viewName, cancellationToken);
            return await materializedViewComments.ToOption().ConfigureAwait(false);
        }
    }
}
