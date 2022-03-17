using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle;

/// <summary>
/// A view provider for Oracle.
/// </summary>
/// <seealso cref="IDatabaseViewProvider" />
public class OracleDatabaseViewProvider : IDatabaseViewProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabaseViewProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
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

    /// <summary>
    /// Gets a query view provider that does not return any materialized views.
    /// </summary>
    /// <value>A query view provider.</value>
    protected IDatabaseViewProvider QueryViewProvider { get; }

    /// <summary>
    /// Gets a materialized view provider, that does not return any simple query views.
    /// </summary>
    /// <value>A materialized view provider.</value>
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
            .OrderBy(static v => v.Name.Schema)
            .ThenBy(static v => v.Name.LocalName);

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
