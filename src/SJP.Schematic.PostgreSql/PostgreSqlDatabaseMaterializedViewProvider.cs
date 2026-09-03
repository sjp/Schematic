using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Queries;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// A materialized view provider for PostgreSQL.
/// </summary>
/// <seealso cref="IDatabaseViewProvider" />
public class PostgreSqlDatabaseMaterializedViewProvider : IDatabaseViewProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDatabaseMaterializedViewProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <param name="identifierResolver">An identifier resolver that enables non-strict name resolution.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <see langword="null" />.</exception>
    public PostgreSqlDatabaseMaterializedViewProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
    }

    /// <summary>
    /// A database connection that is specific to a given PostgreSQL database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// Gets an identifier resolver that enables more relaxed matching against database object names.
    /// </summary>
    /// <value>An identifier resolver.</value>
    protected IIdentifierResolutionStrategy IdentifierResolver { get; }

    /// <summary>
    /// A database connection factory used to query the database.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    /// <summary>
    /// The dialect for the associated database.
    /// </summary>
    /// <value>A database dialect.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// Enumerates all materialized views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of materialized views.</returns>
    public IAsyncEnumerable<IDatabaseView> EnumerateAllViews(CancellationToken cancellationToken = default)
    {
        return DbConnection.QueryEnumerableAsync<GetAllMaterializedViewNames.Result>(GetAllMaterializedViewNames.Sql, cancellationToken)
            .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ViewName))
            .Select(QualifyViewName)
            .SelectAwait(LoadViewAsyncCore);
    }

    /// <summary>
    /// Gets all materialized views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of materialized views.</returns>
    public async Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default)
    {
        var viewNames = await DbConnection.QueryEnumerableAsync<GetAllMaterializedViewNames.Result>(GetAllMaterializedViewNames.Sql, cancellationToken)
            .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ViewName))
            .Select(QualifyViewName)
            .ToListAsync(cancellationToken);

        return await viewNames
            .Select(viewName => LoadViewAsyncCore(viewName, cancellationToken))
            .ToArray()
            .WhenAll();
    }

    /// <summary>
    /// Gets a materialized view.
    /// </summary>
    /// <param name="viewName">A materialized view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A materialized view in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var candidateViewName = QualifyViewName(viewName);
        return LoadView(candidateViewName, cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the materialized view. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="viewName">A materialized view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A materialized view name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedViewName(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var resolvedNames = IdentifierResolver
            .GetResolutionOrder(viewName)
            .Select(QualifyViewName);

        return resolvedNames
            .Select(name => GetResolvedViewNameStrict(name, cancellationToken))
            .FirstSome(cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the materialized view without name resolution. i.e. the name must match strictly to return a result.
    /// </summary>
    /// <param name="viewName">A materialized view name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A materialized view name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedViewNameStrict(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var candidateViewName = QualifyViewName(viewName);
        var qualifiedViewName = DbConnection.QueryFirstOrNone(
            GetMaterializedViewName.Sql,
            new GetMaterializedViewName.Query { SchemaName = candidateViewName.Schema!, ViewName = candidateViewName.LocalName },
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
        var (columns, definition, indexRows, isPopulated) = await (
            LoadColumnsAsync(viewName, cancellationToken),
            LoadDefinitionAsync(viewName, cancellationToken),
            LoadIndexRowsAsync(viewName, cancellationToken),
            LoadIsPopulatedAsync(viewName, cancellationToken)
        ).WhenAll();

        var columnLookup = GetColumnLookup(columns);
        var indexes = PostgreSqlCatalogMapper.MapIndexes(indexRows, columnLookup, Dialect);

        return new DatabaseMaterializedView(
            viewName,
            definition!,
            columns,
            // PostgreSQL does not support triggers on a materialized view.
            [],
            indexes,
            // REFRESH MATERIALIZED VIEW is the only way a PostgreSQL materialized view is brought up to
            // date, and it always recomputes the whole query, so there is no refresh method to report.
            MaterializedViewRefreshMode.OnDemand,
            Option<string>.None,
            isPopulated
        );
    }

    private Task<IEnumerable<GetTableIndexes.Result>> LoadIndexRowsAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        return DbConnection.QueryAsync(
            GetMaterializedViewIndexes.Sql,
            new GetMaterializedViewIndexes.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );
    }

    /// <summary>
    /// Determines whether a materialized view currently holds data, i.e. whether it has been refreshed
    /// since it was created.
    /// </summary>
    /// <param name="viewName">A materialized view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true" /> if the view holds data; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected Task<bool> LoadIsPopulatedAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return DbConnection.ExecuteScalarAsync(
            GetMaterializedViewIsPopulated.Sql,
            new GetMaterializedViewIsPopulated.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );
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
            GetMaterializedViewDefinition.Sql,
            new GetMaterializedViewDefinition.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );
    }

    /// <summary>
    /// Retrieves the columns for a given materialized view.
    /// </summary>
    /// <param name="viewName">A materialized view name.</param>
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
                GetMaterializedViewColumns.Sql,
                new GetMaterializedViewColumns.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
                cancellationToken
            )
            .Select(row =>
            {
                var typeMetadata = PostgreSqlColumnTypeMetadata.Create(
                    Dialect.TypeProvider,
                    new PostgreSqlColumnTypeMetadata.CatalogTypeInfo(
                        row.DataType,
                        row.UdtSchema,
                        row.UdtName,
                        row.DomainSchema,
                        row.DomainName,
                        row.TypeKind,
                        row.ElementTypeSchema,
                        row.ElementTypeName,
                        row.ElementTypeKind,
                        row.EnumLabels),
                    !row.CollationName.IsNullOrWhiteSpace()
                        ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.CollationCatalog, row.CollationSchema, row.CollationName))
                        : Option<Identifier>.None,
                    PostgreSqlColumnTypeMetadata.CreateMaxLength(row.CharacterMaximumLength, row.NumericPrecision, row.NumericPrecisionRadix),
                    PostgreSqlColumnTypeMetadata.CreateNumericPrecision(row.NumericPrecision, row.NumericScale, row.NumericPrecisionRadix),
                    row.DatetimePrecision.HasValue
                        ? Option<int>.Some(row.DatetimePrecision.Value)
                        : Option<int>.None);

                var columnType = Dialect.TypeProvider.CreateColumnType(typeMetadata);
                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
                var autoIncrement = Option<IAutoIncrement>.None;
                var defaultValue = PostgreSqlDefaultValueParser.Parse(row.ColumnDefault);

                return new DatabaseColumn(columnName, columnType, string.Equals(row.IsNullable, Constants.Yes, StringComparison.Ordinal), defaultValue, autoIncrement);
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

    private static class Constants
    {
        public const string Yes = "YES";
    }
}