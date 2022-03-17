using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Query;
using SJP.Schematic.SqlServer.QueryResult;

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
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <c>null</c>.</exception>
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
    protected IDbConnectionFactory DbConnection => Connection.DbConnection;

    /// <summary>
    /// The dialect for the associated database.
    /// </summary>
    /// <value>A database dialect.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// Gets all database views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database views.</returns>
    public virtual async IAsyncEnumerable<IDatabaseView> GetAllViews([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryResult = await DbConnection.QueryAsync<GetAllViewNamesQueryResult>(ViewsQuery, cancellationToken).ConfigureAwait(false);
        var viewNames = queryResult
            .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ViewName))
            .Select(QualifyViewName);

        foreach (var viewName in viewNames)
            yield return await LoadViewAsyncCore(viewName, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// A SQL query that retrieves the names of views available in the database.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string ViewsQuery => ViewsQuerySql;

    private const string ViewsQuerySql = @$"
select schema_name(schema_id) as [{ nameof(GetAllViewNamesQueryResult.SchemaName) }], name as [{ nameof(GetAllViewNamesQueryResult.ViewName) }]
from sys.views
where is_ms_shipped = 0
order by schema_name(schema_id), name";

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

        var candidateViewName = QualifyViewName(viewName);
        return LoadView(candidateViewName, cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the view. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A view name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected OptionAsync<Identifier> GetResolvedViewName(Identifier viewName, CancellationToken cancellationToken)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        var candidateViewName = QualifyViewName(viewName);
        var qualifiedViewName = DbConnection.QueryFirstOrNone<GetViewNameQueryResult>(
            ViewNameQuery,
            new GetViewNameQuery { SchemaName = candidateViewName.Schema!, ViewName = candidateViewName.LocalName },
            cancellationToken
        );

        return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ViewName));
    }

    /// <summary>
    /// A SQL query that retrieves the resolved name of a view in the database.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string ViewNameQuery => ViewNameQuerySql;

    private const string ViewNameQuerySql = @$"
select top 1 schema_name(schema_id) as [{ nameof(GetViewNameQueryResult.SchemaName) }], name as [{ nameof(GetViewNameQueryResult.ViewName) }]
from sys.views
where schema_id = schema_id(@{ nameof(GetViewNameQuery.SchemaName) }) and name = @{ nameof(GetViewNameQuery.ViewName) } and is_ms_shipped = 0";

    /// <summary>
    /// Retrieves a database view, if available.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A view definition, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected virtual OptionAsync<IDatabaseView> LoadView(Identifier viewName, CancellationToken cancellationToken)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        var candidateViewName = QualifyViewName(viewName);
        return GetResolvedViewName(candidateViewName, cancellationToken)
            .MapAsync(name => LoadViewAsyncCore(name, cancellationToken));
    }

    private async Task<IDatabaseView> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var columnsTask = LoadColumnsAsync(viewName, cancellationToken);
        var definitionTask = LoadDefinitionAsync(viewName, cancellationToken);
        await Task.WhenAll(columnsTask, definitionTask).ConfigureAwait(false);

        var columns = await columnsTask.ConfigureAwait(false);
        var definition = await definitionTask.ConfigureAwait(false);
        var isMaterialized = await LoadIndexExistsAsync(viewName, cancellationToken).ConfigureAwait(false);

        return isMaterialized
            ? new DatabaseMaterializedView(viewName, definition, columns)
            : new DatabaseView(viewName, definition, columns);
    }

    /// <summary>
    /// Retrieves the definition of a view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A string representing the definition of a view.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected virtual Task<string> LoadDefinitionAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        return DbConnection.ExecuteScalarAsync<string>(
            DefinitionQuery,
            new GetViewDefinitionQuery { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );
    }

    /// <summary>
    /// A SQL query that retrieves the definition of a view.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string DefinitionQuery => DefinitionQuerySql;

    private const string DefinitionQuerySql = @$"
select sm.definition
from sys.sql_modules sm
inner join sys.views v on sm.object_id = v.object_id
where schema_name(v.schema_id) = @{ nameof(GetViewDefinitionQuery.SchemaName) } and v.name = @{ nameof(GetViewDefinitionQuery.ViewName) } and v.is_ms_shipped = 0";

    /// <summary>
    /// Determines whether the view is an indexed view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the view is indexed; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected virtual Task<bool> LoadIndexExistsAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        return DbConnection.ExecuteScalarAsync<bool>(
            IndexExistsQuery,
            new GetViewIndexExistsQuery { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );
    }

    /// <summary>
    /// A SQL query that retrieves whether indexes are present on a view.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string IndexExistsQuery => IndexExistsQuerySql;

    private const string IndexExistsQuerySql = @$"
select top 1 1
from sys.views v
inner join sys.indexes i on v.object_id = i.object_id
where schema_name(v.schema_id) = @{ nameof(GetViewIndexExistsQuery.SchemaName) } and v.name = @{ nameof(GetViewIndexExistsQuery.ViewName) } and v.is_ms_shipped = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore";

    /// <summary>
    /// Retrieves the columns for a given view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An ordered collection of columns.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected virtual Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        return LoadColumnsAsyncCore(viewName, cancellationToken);
    }

    private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var query = await DbConnection.QueryAsync<GetViewColumnsQueryResult>(
            ColumnsQuery,
            new GetViewColumnsQuery { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        ).ConfigureAwait(false);

        var result = new List<IDatabaseColumn>();

        foreach (var row in query)
        {
            var typeMetadata = new ColumnTypeMetadata
            {
                TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
                Collation = !row.Collation.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                    : Option<Identifier>.None,
                MaxLength = row.MaxLength,
                NumericPrecision = new NumericPrecision(row.Precision, row.Scale)
            };
            var columnType = Dialect.TypeProvider.CreateColumnType(typeMetadata);

            var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
            var autoIncrement = row.IdentityIncrement
                .Match(
                    incr => row.IdentitySeed.Match(seed => new AutoIncrement(seed, incr), () => Option<IAutoIncrement>.None),
                    static () => Option<IAutoIncrement>.None
                );
            var defaultValue = row.HasDefaultValue && row.DefaultValue != null
                ? Option<string>.Some(row.DefaultValue)
                : Option<string>.None;

            var column = new DatabaseColumn(columnName, columnType, row.IsNullable, defaultValue, autoIncrement);

            result.Add(column);
        }

        return result;
    }

    /// <summary>
    /// A SQL query that retrieves column definitions.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string ColumnsQuery => ColumnsQuerySql;

    private const string ColumnsQuerySql = @$"
select
    c.name as [{ nameof(GetViewColumnsQueryResult.ColumnName) }],
    schema_name(st.schema_id) as [{ nameof(GetViewColumnsQueryResult.ColumnTypeSchema) }],
    st.name as [{ nameof(GetViewColumnsQueryResult.ColumnTypeName) }],
    c.max_length as [{ nameof(GetViewColumnsQueryResult.MaxLength) }],
    c.precision as [{ nameof(GetViewColumnsQueryResult.Precision) }],
    c.scale as [{ nameof(GetViewColumnsQueryResult.Scale) }],
    c.collation_name as [{ nameof(GetViewColumnsQueryResult.Collation) }],
    c.is_computed as [{ nameof(GetViewColumnsQueryResult.IsComputed) }],
    c.is_nullable as [{ nameof(GetViewColumnsQueryResult.IsNullable) }],
    dc.parent_column_id as [{ nameof(GetViewColumnsQueryResult.HasDefaultValue) }],
    dc.definition as [{ nameof(GetViewColumnsQueryResult.DefaultValue) }],
    cc.definition as [{ nameof(GetViewColumnsQueryResult.ComputedColumnDefinition) }],
    (convert(bigint, ic.seed_value)) as [{ nameof(GetViewColumnsQueryResult.IdentitySeed) }],
    (convert(bigint, ic.increment_value)) as [{ nameof(GetViewColumnsQueryResult.IdentityIncrement) }]
from sys.views v
inner join sys.columns c on v.object_id = c.object_id
left join sys.default_constraints dc on c.object_id = dc.parent_object_id and c.column_id = dc.parent_column_id
left join sys.computed_columns cc on c.object_id = cc.object_id and c.column_id = cc.column_id
left join sys.identity_columns ic on c.object_id = ic.object_id and c.column_id = ic.column_id
left join sys.types st on c.user_type_id = st.user_type_id
where schema_name(v.schema_id) = @{ nameof(GetViewColumnsQuery.SchemaName) } and v.name = @{ nameof(GetViewColumnsQuery.ViewName) } and v.is_ms_shipped = 0
order by c.column_id";

    /// <summary>
    /// Qualifies the name of the view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <returns>A view name is at least as qualified as the given view name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected Identifier QualifyViewName(Identifier viewName)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        var schema = viewName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, viewName.LocalName);
    }
}
