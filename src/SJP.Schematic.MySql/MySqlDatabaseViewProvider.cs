using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Nito.AsyncEx;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.MySql.Queries;

namespace SJP.Schematic.MySql;

/// <summary>
/// A view provider for MySQL.
/// </summary>
/// <seealso cref="IDatabaseViewProvider" />
/// <remarks>
/// MySQL lists a view to any user with a privilege on it, but only shows its definition to users that also hold
/// <c>SHOW VIEW</c>. Such a view is still returned, with a SQL comment in place of its definition.
/// </remarks>
public class MySqlDatabaseViewProvider : IDatabaseViewProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDatabaseViewProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <see langword="null" />.</exception>
    public MySqlDatabaseViewProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));

        // a failed load is not remembered, otherwise one transient error would break every later view load
        _catalogFeatures = new AsyncLazy<GetCatalogFeatures.Result>(LoadCatalogFeatures, AsyncLazyFlags.RetryOnFailure);
    }

    /// <summary>
    /// A database connection that is specific to a given MySQL database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// A database connection factory to query the database.
    /// </summary>
    /// <value>A connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    /// <summary>
    /// A dialect for the MySQL connection.
    /// </summary>
    /// <value>A database dialect.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// Enumerates all database views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database views.</returns>
    public async IAsyncEnumerable<IDatabaseView> EnumerateAllViews([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryResult = DbConnection.QueryEnumerableAsync(
            GetAllViewNames.Sql,
            new GetAllViewNames.Query { SchemaName = IdentifierDefaults.Schema! },
            cancellationToken
        );

        var viewNames = await queryResult
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ViewName))
            .Select(QualifyViewName)
            .ToListAsync(cancellationToken);

        var results = viewNames.SelectOrderedPrefetchAsync(LoadViewAsyncCore, Math.Max(1, DbConnection.MaxConcurrentQueries), cancellationToken);

        await foreach (var result in results.WithCancellation(cancellationToken))
            yield return result;
    }

    /// <summary>
    /// Gets all database views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database views.</returns>
    public async Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default)
    {
        var queryResult = DbConnection.QueryEnumerableAsync(
            GetAllViewNames.Sql,
            new GetAllViewNames.Query { SchemaName = IdentifierDefaults.Schema! },
            cancellationToken
        );

        var viewNames = await queryResult
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ViewName))
            .Select(QualifyViewName)
            .ToListAsync(cancellationToken);

        return await viewNames.SelectBoundedAsync(LoadViewAsyncCore, Math.Max(1, DbConnection.MaxConcurrentQueries), cancellationToken);
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
    /// <param name="viewName">A view name that will be resolved.</param>
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

    /// <summary>) }
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
        var details = await LoadViewDetailsAsync(viewName, cancellationToken);

        // MySQL supports neither triggers on a view nor indexing one.
        return new DatabaseView(
            viewName,
            !details.Definition.IsNullOrWhiteSpace()
                ? details.Definition
                : UnavailableDefinition,
            details.Columns,
            [],
            [],
            details.CheckOption,
            details.IsUpdatable
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
        var details = await LoadViewDetailsAsync(viewName, cancellationToken);
        return details.Columns;
    }

    // A view's definition, options and columns are read with one query. Its definition row is missing
    // when the view was dropped after its name was read.
    private async Task<ViewDetails> LoadViewDetailsAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        var rows = await DbConnection.QueryAsync(
            GetViewDetails.Sql,
            new GetViewDetails.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );

        GetViewDetails.Result? definitionRow = null;
        var columnRows = new List<GetViewDetails.Result>();
        foreach (var row in rows)
        {
            if (row.Ordinal == null)
                definitionRow = row;
            else
                columnRows.Add(row);
        }

        // the union gives no order, so the columns are put back in the order the view declares them
        columnRows.Sort(static (a, b) => a.Ordinal!.Value.CompareTo(b.Ordinal!.Value));
        var features = await _catalogFeatures;
        var columns = columnRows.ConvertAll(row => CreateColumn(row, features.IsMariaDb));

        if (definitionRow == null)
            return new ViewDetails(null, ViewCheckOption.None, false, columns);

        var checkOption = definitionRow.CheckOption != null && CheckOptionMapping.TryGetValue(definitionRow.CheckOption, out var mappedCheckOption)
            ? mappedCheckOption
            : ViewCheckOption.None;
        var isUpdatable = string.Equals(definitionRow.IsUpdatable, Constants.Yes, StringComparison.OrdinalIgnoreCase);

        return new ViewDetails(definitionRow.Definition, checkOption, isUpdatable, columns);
    }

    private sealed record ViewDetails(string? Definition, ViewCheckOption CheckOption, bool IsUpdatable, IReadOnlyList<IDatabaseColumn> Columns);

    private IDatabaseColumn CreateColumn(GetViewDetails.Result row, bool isMariaDb)
    {
        var typeMetadata = MySqlColumnTypeMetadata.Create(
            row.DataTypeName!,
            row.ColumnType,
            !row.Collation.IsNullOrWhiteSpace()
                ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                : Option<Identifier>.None,
            row.CharacterMaxLength ?? 0,
            new NumericPrecision(row.Precision ?? 0, row.Scale ?? 0),
            row.DateTimePrecision.HasValue
                ? Option<int>.Some(row.DateTimePrecision.Value)
                : Option<int>.None);
        var columnType = Dialect.TypeProvider.CreateColumnType(typeMetadata);

        var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName!);
        var isAutoIncrement = row.ExtraInformation?.Contains(Constants.AutoIncrement, StringComparison.OrdinalIgnoreCase) == true;
        var autoIncrement = isAutoIncrement
            ? Option<IAutoIncrement>.Some(new AutoIncrement(1, 1))
            : Option<IAutoIncrement>.None;
        var isNullable = !string.Equals(row.IsNullable, Constants.No, StringComparison.OrdinalIgnoreCase);
        var defaultValue = MySqlDefaultValueParser.Parse(row.DefaultValue, row.ExtraInformation, isMariaDb);

        return new DatabaseColumn(columnName, columnType, isNullable, defaultValue, autoIncrement);
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

    // MySQL and MariaDB report column defaults differently, so the server flavour is read once per provider.
    private Task<GetCatalogFeatures.Result> LoadCatalogFeatures() =>
        DbConnection.QuerySingleAsync<GetCatalogFeatures.Result>(GetCatalogFeatures.Sql, CancellationToken.None);

    private readonly AsyncLazy<GetCatalogFeatures.Result> _catalogFeatures;

    // information_schema.views.view_definition is empty when the user lacks SHOW VIEW, and the row
    // is missing when the view was dropped after its name was read
    private const string UnavailableDefinition = "-- definition unavailable (requires the SHOW VIEW privilege)";

    private static class Constants
    {
        public const string AutoIncrement = "auto_increment";

        public const string No = "NO";

        public const string Yes = "YES";
    }

    // information_schema.views.check_option values
    private static readonly IReadOnlyDictionary<string, ViewCheckOption> CheckOptionMapping = new Dictionary<string, ViewCheckOption>(StringComparer.OrdinalIgnoreCase)
    {
        ["NONE"] = ViewCheckOption.None,
        ["LOCAL"] = ViewCheckOption.Local,
        ["CASCADED"] = ViewCheckOption.Cascaded,
    };
}
