using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

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
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <see langword="null" />.</exception>
    public OracleDatabaseViewProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(identifierDefaults);
        ArgumentNullException.ThrowIfNull(identifierResolver);

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
    /// Enumerates all database views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database views.</returns>
    public IAsyncEnumerable<IDatabaseView> EnumerateAllViews(CancellationToken cancellationToken = default)
    {
        return QueryViewProvider.EnumerateAllViews(cancellationToken)
            .Concat(MaterializedViewProvider.EnumerateAllViews(cancellationToken))
            .OrderBy(static v => v.Name.Schema, StringComparer.Ordinal)
            .ThenBy(static v => v.Name.LocalName, StringComparer.Ordinal);
    }

    /// <summary>
    /// Gets all database views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database views.</returns>
    public async Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default)
    {
        var (queryViews, materializedViews) = await (
            QueryViewProvider.GetAllViews(cancellationToken),
            MaterializedViewProvider.GetAllViews(cancellationToken)
        ).WhenAll().ConfigureAwait(false);

        return queryViews
            .Concat(materializedViews)
            .OrderBy(static v => v.Name.Schema, StringComparer.Ordinal)
            .ThenBy(static v => v.Name.LocalName, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// Gets a database view.
    /// </summary>
    /// <param name="viewName">A database view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database view in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return QueryViewProvider.GetView(viewName, cancellationToken)
             | MaterializedViewProvider.GetView(viewName, cancellationToken);
    }
}