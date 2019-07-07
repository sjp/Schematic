using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Oracle.Comments
{
    public class OracleViewCommentProvider : IDatabaseViewCommentProvider
    {
        public OracleViewCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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

        public async Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments(CancellationToken cancellationToken = default)
        {
            var queryViewCommentsTask = QueryViewCommentProvider.GetAllViewComments(cancellationToken);
            var materializedViewCommentsTask = MaterializedViewCommentProvider.GetAllViewComments(cancellationToken);
            await Task.WhenAll(queryViewCommentsTask, materializedViewCommentsTask).ConfigureAwait(false);

            var queryViewComments = queryViewCommentsTask.Result;
            var materializedViewComments = materializedViewCommentsTask.Result;

            return queryViewComments
                .Concat(materializedViewComments)
                .OrderBy(v => v.ViewName.Schema)
                .ThenBy(v => v.ViewName.LocalName)
                .ToList();
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
