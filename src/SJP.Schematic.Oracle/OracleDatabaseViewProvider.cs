using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Query;

namespace SJP.Schematic.Oracle
{
    public class OracleDatabaseViewProvider : IDatabaseViewProvider
    {
        public OracleDatabaseViewProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver, IDbTypeProvider typeProvider)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (identifierDefaults == null)
                throw new ArgumentNullException(nameof(identifierDefaults));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));
            if (typeProvider == null)
                throw new ArgumentNullException(nameof(typeProvider));

            QueryViewProvider = new OracleDatabaseQueryViewProvider(connection, identifierDefaults, identifierResolver, typeProvider);
            MaterializedViewProvider = new OracleDatabaseMaterializedViewProvider(connection, identifierDefaults, identifierResolver, typeProvider);
        }

        protected IDatabaseViewProvider QueryViewProvider { get; }

        protected IDatabaseViewProvider MaterializedViewProvider { get; }

        public async Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryViewsTask = QueryViewProvider.GetAllViews(cancellationToken);
            var materializedViewsTask = MaterializedViewProvider.GetAllViews(cancellationToken);
            await Task.WhenAll(queryViewsTask, materializedViewsTask).ConfigureAwait(false);

            var queryViews = queryViewsTask.Result;
            var materializedViews = materializedViewsTask.Result;

            return queryViews
                .Concat(materializedViews)
                .OrderBy(v => v.Name.Schema)
                .ThenBy(v => v.Name.LocalName)
                .ToList();
        }

        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return QueryViewProvider.GetView(viewName, cancellationToken)
                 | MaterializedViewProvider.GetView(viewName, cancellationToken);
        }
    }
}
