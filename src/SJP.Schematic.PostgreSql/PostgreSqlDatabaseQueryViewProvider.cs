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
/// A query view provider for PostgreSQL.
/// </summary>
/// <seealso cref="IDatabaseViewProvider" />
public class PostgreSqlDatabaseQueryViewProvider : IDatabaseViewProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDatabaseQueryViewProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <param name="identifierResolver">An identifier resolver that enables non-strict name resolution.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <see langword="null" />.</exception>
    public PostgreSqlDatabaseQueryViewProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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

        var resolvedNames = IdentifierResolver
            .GetResolutionOrder(viewName)
            .Select(QualifyViewName);

        return resolvedNames
            .Select(name => GetResolvedViewNameStrict(name, cancellationToken))
            .FirstSome(cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the view without name resolution. i.e. the name must match strictly to return a result.
    /// </summary>
    /// <param name="viewName">A view name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A view name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedViewNameStrict(Identifier viewName, CancellationToken cancellationToken)
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
        var (columns, definition, triggers, options) = await (
            LoadColumnsAsync(viewName, cancellationToken),
            LoadDefinitionAsync(viewName, cancellationToken),
            LoadTriggersAsync(viewName, cancellationToken),
            LoadOptionsAsync(viewName, cancellationToken)
        ).WhenAll();

        // PostgreSQL cannot index a plain view, only a materialized one.
        return new DatabaseView(viewName, definition!, columns, triggers, [], options.CheckOption, options.IsUpdatable);
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

        return PostgreSqlCatalogMapper.MapTriggers(queryResult);
    }

    /// <summary>
    /// Retrieves the check option and updatability of a view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The view's check option, and whether rows can be written through it.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected Task<(ViewCheckOption CheckOption, bool IsUpdatable)> LoadOptionsAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return LoadOptionsAsyncCore(viewName, cancellationToken);
    }

    private async Task<(ViewCheckOption CheckOption, bool IsUpdatable)> LoadOptionsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var options = await DbConnection.QueryFirstOrNone(
            GetViewOptions.Sql,
            new GetViewOptions.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        ).ToOption();

        return options.Match(
            static row =>
            (
                CheckOptionMapping.TryGetValue(row.CheckOption, out var checkOption) ? checkOption : ViewCheckOption.None,
                string.Equals(row.IsUpdatable, Constants.Yes, StringComparison.Ordinal)
            ),
            static () => (ViewCheckOption.None, false)
        );
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

    // information_schema.views.check_option values
    private static readonly IReadOnlyDictionary<string, ViewCheckOption> CheckOptionMapping = new Dictionary<string, ViewCheckOption>(StringComparer.OrdinalIgnoreCase)
    {
        ["NONE"] = ViewCheckOption.None,
        ["LOCAL"] = ViewCheckOption.Local,
        ["CASCADED"] = ViewCheckOption.Cascaded,
    };

    private static class Constants
    {
        public const string Yes = "YES";
    }
}