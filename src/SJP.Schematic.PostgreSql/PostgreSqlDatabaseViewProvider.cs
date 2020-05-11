using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlDatabaseViewProvider : IDatabaseViewProvider
    {
        public PostgreSqlDatabaseViewProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (identifierDefaults == null)
                throw new ArgumentNullException(nameof(identifierDefaults));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            QueryViewProvider = new PostgreSqlDatabaseQueryViewProvider(connection, identifierDefaults, identifierResolver);
            MaterializedViewProvider = new PostgreSqlDatabaseMaterializedViewProvider(connection, identifierDefaults, identifierResolver);
        }

        protected IDatabaseViewProvider QueryViewProvider { get; }

        protected IDatabaseViewProvider MaterializedViewProvider { get; }

        /// <summary>
        /// Gets all database views.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database views.</returns>
        public async IAsyncEnumerable<IDatabaseView> GetAllViews([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryViews = await QueryViewProvider.GetAllViews(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var materializedViews = await MaterializedViewProvider.GetAllViews(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);

            var views = queryViews
                .Concat(materializedViews)
                .OrderBy(v => v.Name.Schema)
                .ThenBy(v => v.Name.LocalName);

            foreach (var view in views)
                yield return view;
        }

        /// <summary>
        /// Gets a database view.
        /// </summary>
        /// <param name="viewName">A database view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database view in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return GetViewAsyncCore(viewName, cancellationToken).ToAsync();
        }

        // This is equivalent to the '|' operator on OptionAsync<T> objects.
        // However, this method is necessary because we need to set ConfigureAwait(false)
        // on the async task, which the built-in '|' operator does not do.
        // Without this method we get InProgressException errors...
        private async Task<Option<IDatabaseView>> GetViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var queryView = QueryViewProvider.GetView(viewName, cancellationToken);
            var queryViewIsSome = await queryView.IsSome.ConfigureAwait(false);
            if (queryViewIsSome)
                return await queryView.ToOption().ConfigureAwait(false);

            var materializedView = MaterializedViewProvider.GetView(viewName, cancellationToken);
            return await materializedView.ToOption().ConfigureAwait(false);
        }
    }
}
