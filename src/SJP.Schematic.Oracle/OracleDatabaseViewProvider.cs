using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public class OracleDatabaseViewProvider : IDatabaseViewProvider
    {
        public OracleDatabaseViewProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (identifierDefaults == null)
                throw new ArgumentNullException(nameof(identifierDefaults));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            QueryViewProvider = new OracleDatabaseQueryViewProvider(connection, identifierDefaults, identifierResolver);
            MaterializedViewProvider = new OracleDatabaseMaterializedViewProvider(connection, identifierDefaults, identifierResolver);
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
            var queryViewsTask = QueryViewProvider.GetAllViews(cancellationToken).ToListAsync(cancellationToken).AsTask();
            var materializedViewsTask = MaterializedViewProvider.GetAllViews(cancellationToken).ToListAsync(cancellationToken).AsTask();
            await Task.WhenAll(queryViewsTask, materializedViewsTask).ConfigureAwait(false);

            var queryViews = await queryViewsTask.ConfigureAwait(false);
            var materializedViews = await materializedViewsTask.ConfigureAwait(false);

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

            return QueryViewProvider.GetView(viewName, cancellationToken)
                 | MaterializedViewProvider.GetView(viewName, cancellationToken);
        }
    }
}
