using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// A view provider for PostgreSQL.
/// </summary>
/// <seealso cref="IDatabaseViewProvider" />
public class PostgreSqlDatabaseViewProvider : IDatabaseViewProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDatabaseViewProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <see langword="null" />.</exception>
    public PostgreSqlDatabaseViewProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(identifierDefaults);
        ArgumentNullException.ThrowIfNull(identifierResolver);

        _connectionFactory = connection.ConnectionFactory;
        _queryViewProvider = new PostgreSqlDatabaseQueryViewProvider(connection, identifierDefaults, identifierResolver);
        _materializedViewProvider = new PostgreSqlDatabaseMaterializedViewProvider(connection, identifierDefaults, identifierResolver);
        QueryViewProvider = _queryViewProvider;
        MaterializedViewProvider = _materializedViewProvider;
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
    public async IAsyncEnumerable<IDatabaseView> EnumerateAllViews([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (queryViewNames, materializedViewNames) = await (
            _queryViewProvider.GetAllViewNamesAsync(cancellationToken),
            _materializedViewProvider.GetAllViewNamesAsync(cancellationToken)
        ).WhenAll();

        // order the names rather than the loaded views, so that each view can be returned as soon as it is loaded
        var viewNames = queryViewNames
            .Select(static name => (Name: name, IsMaterialized: false))
            .Concat(materializedViewNames.Select(static name => (Name: name, IsMaterialized: true)))
            .OrderBy(static v => v.Name.Schema, StringComparer.Ordinal)
            .ThenBy(static v => v.Name.LocalName, StringComparer.Ordinal);

        var views = viewNames.SelectOrderedPrefetchAsync(LoadViewAsyncCore, Math.Max(1, _connectionFactory.MaxConcurrentQueries), cancellationToken);

        await foreach (var view in views.WithCancellation(cancellationToken))
            yield return view;
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
        ).WhenAll();

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

    private Task<IDatabaseView> LoadViewAsyncCore((Identifier Name, bool IsMaterialized) view, CancellationToken cancellationToken)
    {
        return view.IsMaterialized
            ? _materializedViewProvider.LoadViewAsyncCore(view.Name, cancellationToken)
            : _queryViewProvider.LoadViewAsyncCore(view.Name, cancellationToken);
    }

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly PostgreSqlDatabaseQueryViewProvider _queryViewProvider;
    private readonly PostgreSqlDatabaseMaterializedViewProvider _materializedViewProvider;
}