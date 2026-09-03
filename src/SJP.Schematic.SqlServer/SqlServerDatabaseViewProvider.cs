using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Queries;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// A view provider for SQL Server.
/// </summary>
/// <seealso cref="IDatabaseViewProvider" />
public class SqlServerDatabaseViewProvider : IDatabaseViewProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDatabaseViewProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <see langword="null" />.</exception>
    public SqlServerDatabaseViewProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    /// <summary>
    /// A database connection that is specific to a given SQL Server database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults for the given database.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// A database connection factory.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    /// <summary>
    /// The dialect for the associated database.
    /// </summary>
    /// <value>A database dialect.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// Enumerates all database views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database views.</returns>
    public IAsyncEnumerable<IDatabaseView> EnumerateAllViews(CancellationToken cancellationToken = default)
    {
        return DbConnection.QueryEnumerableAsync<GetAllViewNames.Result>(GetAllViewNames.Sql, cancellationToken)
            .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ViewName))
            .Select(QualifyViewName)
            .SelectAwait(LoadViewAsyncCore);
    }

    /// <summary>
    /// Gets all database views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database views.</returns>
    public async Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default)
    {
        var viewNames = await DbConnection.QueryEnumerableAsync<GetAllViewNames.Result>(GetAllViewNames.Sql, cancellationToken)
            .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ViewName))
            .Select(QualifyViewName)
            .ToListAsync(cancellationToken);

        return await viewNames
            .Select(viewName => LoadViewAsyncCore(viewName, cancellationToken))
            .ToArray()
            .WhenAll();
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

        var candidateViewName = QualifyViewName(viewName);
        return LoadView(candidateViewName, cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the view. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A view name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedViewName(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var candidateViewName = QualifyViewName(viewName);
        var qualifiedViewName = DbConnection.QueryFirstOrNone(
            GetViewName.Sql,
            new GetViewName.Query { SchemaName = candidateViewName.Schema!, ViewName = candidateViewName.LocalName },
            cancellationToken
        );

        return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ViewName));
    }

    /// <summary>
    /// Retrieves a database view, if available.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A view definition, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected OptionAsync<IDatabaseView> LoadView(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var candidateViewName = QualifyViewName(viewName);
        return GetResolvedViewName(candidateViewName, cancellationToken)
            .MapAsync(name => LoadViewAsyncCore(name, cancellationToken));
    }

    private async Task<IDatabaseView> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var (
            columns,
            definition,
            triggers,
            indexRows,
            checkOption
        ) = await (
            LoadColumnsAsync(viewName, cancellationToken),
            LoadDefinitionAsync(viewName, cancellationToken),
            LoadTriggersAsync(viewName, cancellationToken),
            LoadIndexRowsAsync(viewName, cancellationToken),
            LoadCheckOptionAsync(viewName, cancellationToken)
        ).WhenAll();

        var columnLookup = GetColumnLookup(columns);
        var indexes = SqlServerCatalogMapper.MapIndexes(indexRows, columnLookup, Dialect);

        // SQL Server has no materialized view of its own. An indexed view is the equivalent: creating a
        // unique clustered index on a view is what causes its results to be stored.
        return indexes.Count > 0
            ? new DatabaseMaterializedView(
                viewName,
                definition!,
                columns,
                triggers,
                indexes,
                // an indexed view is kept in step with its base tables as they are modified, and is
                // therefore always populated. There is no refresh to request or method to choose.
                MaterializedViewRefreshMode.OnCommit,
                Option<string>.None,
                true
            )
            // SQL Server does not record whether a view is updatable: INFORMATION_SCHEMA.VIEWS.IS_UPDATABLE
            // always reports 'NO', and no sys catalog view carries the information, so it is left unset.
            : new DatabaseView(viewName, definition!, columns, triggers, indexes, checkOption, isUpdatable: false);
    }

    private static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(IReadOnlyList<IDatabaseColumn> columns)
    {
        var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

        foreach (var column in columns)
        {
            if (column.Name != null)
                result[column.Name.LocalName] = column;
        }

        return result;
    }

    /// <summary>
    /// Retrieves the definition of a view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A string representing the definition of a view.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected Task<string?> LoadDefinitionAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return DbConnection.ExecuteScalarAsync(
            GetViewDefinition.Sql,
            new GetViewDefinition.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );
    }

    /// <summary>
    /// Retrieves the triggers defined on a view, i.e. its <c>INSTEAD OF</c> triggers.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of triggers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return LoadTriggersAsyncCore(viewName, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync(
            GetViewTriggers.Sql,
            new GetViewTriggers.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );

        return SqlServerCatalogMapper.MapTriggers(queryResult);
    }

    private Task<IEnumerable<GetTableIndexes.Result>> LoadIndexRowsAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        return DbConnection.QueryAsync(
            GetViewIndexes.Sql,
            new GetViewIndexes.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );
    }

    /// <summary>
    /// Retrieves the check option that constrains rows written through a view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A check option.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected Task<ViewCheckOption> LoadCheckOptionAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return LoadCheckOptionAsyncCore(viewName, cancellationToken);
    }

    private async Task<ViewCheckOption> LoadCheckOptionAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var checkOption = await DbConnection.ExecuteScalarAsync(
            GetViewCheckOption.Sql,
            new GetViewCheckOption.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );

        // SQL Server only supports a cascaded check option, which it reports as 'CASCADE'.
        return string.Equals(checkOption, "CASCADE", StringComparison.OrdinalIgnoreCase)
            ? ViewCheckOption.Cascaded
            : ViewCheckOption.None;
    }

    /// <summary>
    /// Retrieves the columns for a given view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An ordered collection of columns.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return LoadColumnsAsyncCore(viewName, cancellationToken);
    }

    private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        return await DbConnection.QueryEnumerableAsync(
                GetViewColumns.Sql,
                new GetViewColumns.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
                cancellationToken
            )
            .Select(row =>
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
                    Collation = !row.Collation.IsNullOrWhiteSpace()
                        ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                        : Option<Identifier>.None,
                    MaxLength = row.MaxLength,
                    NumericPrecision = new NumericPrecision(row.Precision, row.Scale),
                };
                var columnType = Dialect.TypeProvider.CreateColumnType(typeMetadata);

                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);

                // views cannot own default constraints or identity columns, so these are always absent
                return new DatabaseColumn(columnName, columnType, row.IsNullable, Option<string>.None, Option<IAutoIncrement>.None);
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Qualifies the name of the view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <returns>A view name is at least as qualified as the given view name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected Identifier QualifyViewName(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var schema = viewName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, viewName.LocalName);
    }
}