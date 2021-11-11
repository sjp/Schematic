using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Query;
using SJP.Schematic.Oracle.QueryResult;

namespace SJP.Schematic.Oracle
{
    /// <summary>
    /// A materialized view provider for Oracle.
    /// </summary>
    /// <seealso cref="IDatabaseViewProvider" />
    public class OracleDatabaseMaterializedViewProvider : IDatabaseViewProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleDatabaseMaterializedViewProvider"/> class.
        /// </summary>
        /// <param name="connection">A schematic connection.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
        public OracleDatabaseMaterializedViewProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        /// <summary>
        /// A database connection that is specific to a given Oracle database.
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
        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        /// <summary>
        /// The dialect for the associated database.
        /// </summary>
        /// <value>A database dialect.</value>
        protected IDatabaseDialect Dialect => Connection.Dialect;

        /// <summary>
        /// Gets all materialized views.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of materialized views.</returns>
        public virtual async IAsyncEnumerable<IDatabaseView> GetAllViews([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResult = await DbConnection.QueryAsync<GetAllMaterializedViewNamesQueryResult>(ViewsQuery, cancellationToken).ConfigureAwait(false);
            var viewNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ViewName))
                .Select(QualifyViewName);

            foreach (var name in viewNames)
                yield return await LoadViewAsyncCore(name, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// A SQL query that retrieves the names of materialized views available in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ViewsQuery => ViewsQuerySql;

        private const string ViewsQuerySql = @$"
select
    mv.OWNER as ""{ nameof(GetAllMaterializedViewNamesQueryResult.SchemaName) }"",
    mv.MVIEW_NAME as ""{ nameof(GetAllMaterializedViewNamesQueryResult.ViewName) }""
from SYS.ALL_MVIEWS mv
inner join SYS.ALL_OBJECTS o on mv.OWNER = o.OWNER and mv.MVIEW_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y' and o.OBJECT_TYPE <> 'TABLE'
order by mv.OWNER, mv.MVIEW_NAME";

        /// <summary>
        /// Gets a materialized view.
        /// </summary>
        /// <param name="viewName">A materialized view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A materialized view in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            return LoadView(candidateViewName, cancellationToken);
        }

        /// <summary>
        /// Gets the resolved name of the materialized view. This enables non-strict name matching to be applied.
        /// </summary>
        /// <param name="viewName">A materialized view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A materialized view name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedViewName(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

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
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedViewNameStrict(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            var qualifiedViewName = DbConnection.QueryFirstOrNone<GetMaterializedViewNameQueryResult>(
                ViewNameQuery,
                new GetMaterializedViewNameQuery { SchemaName = candidateViewName.Schema!, ViewName = candidateViewName.LocalName },
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
select mv.OWNER as ""{ nameof(GetMaterializedViewNameQueryResult.SchemaName) }"", mv.MVIEW_NAME as ""{ nameof(GetMaterializedViewNameQueryResult.ViewName) }""
from SYS.ALL_MVIEWS mv
inner join SYS.ALL_OBJECTS o on mv.OWNER = o.OWNER and mv.MVIEW_NAME = o.OBJECT_NAME
where mv.OWNER = :{ nameof(GetMaterializedViewNameQuery.SchemaName) } and mv.MVIEW_NAME = :{ nameof(GetMaterializedViewNameQuery.ViewName) }
    and o.ORACLE_MAINTAINED <> 'Y' and o.OBJECT_TYPE <> 'TABLE'";

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

            return new DatabaseMaterializedView(viewName, definition, columns);
        }
        /// <summary>
        /// Retrieves the definition of a materialized view.
        /// </summary>
        /// <param name="viewName">A materialized view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A string representing the definition of a materialized view.</returns>
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
        /// A SQL query that retrieves the definition of a materialized view.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string DefinitionQuery => DefinitionQuerySql;

        private const string DefinitionQuerySql = @$"
select QUERY
from SYS.ALL_MVIEWS
where OWNER = :{ nameof(GetViewDefinitionQuery.SchemaName) } and MVIEW_NAME = :{ nameof(GetViewDefinitionQuery.ViewName) }";

        /// <summary>
        /// Retrieves the columns for a given materialized view.
        /// </summary>
        /// <param name="viewName">A materialized view name.</param>
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
            var query = await DbConnection.QueryAsync<GetMaterializedViewColumnsQueryResult>(
                ColumnsQuery,
                new GetMaterializedViewColumnsQuery { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var columnNames = query
                .Where(static row => row.ColumnName != null)
                .Select(static row => row.ColumnName!)
                .ToList();
            var notNullableColumnNames = await GetNotNullConstrainedColumnsAsync(viewName, columnNames, cancellationToken).ConfigureAwait(false);
            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
                    Collation = !row.Collation.IsNullOrWhiteSpace()
                        ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                        : Option<Identifier>.None,
                    MaxLength = row.DataLength,
                    NumericPrecision = row.Precision > 0 || row.Scale > 0
                        ? Option<INumericPrecision>.Some(new NumericPrecision(row.Precision, row.Scale))
                        : Option<INumericPrecision>.None
                };
                var columnType = Dialect.TypeProvider.CreateColumnType(typeMetadata);

                var isNullable = !notNullableColumnNames.Contains(row.ColumnName);
                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
                var defaultValue = !row.DefaultValue.IsNullOrWhiteSpace()
                    ? Option<string>.Some(row.DefaultValue)
                    : Option<string>.None;

                var column = new OracleDatabaseColumn(columnName, columnType, isNullable, defaultValue);

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
    atc.COLUMN_NAME as ""{ nameof(GetMaterializedViewColumnsQueryResult.ColumnName) }"",
    atc.DATA_TYPE_OWNER as ""{ nameof(GetMaterializedViewColumnsQueryResult.ColumnTypeSchema) }"",
    atc.DATA_TYPE as ""{ nameof(GetMaterializedViewColumnsQueryResult.ColumnTypeName) }"",
    atc.DATA_LENGTH as ""{ nameof(GetMaterializedViewColumnsQueryResult.DataLength) }"",
    atc.DATA_PRECISION as ""{ nameof(GetMaterializedViewColumnsQueryResult.Precision) }"",
    atc.DATA_SCALE as ""{ nameof(GetMaterializedViewColumnsQueryResult.Scale) }"",
    atc.DATA_DEFAULT as ""{ nameof(GetMaterializedViewColumnsQueryResult.DefaultValue) }"",
    atc.CHAR_LENGTH as ""{ nameof(GetMaterializedViewColumnsQueryResult.CharacterLength) }"",
    atc.CHARACTER_SET_NAME as ""{ nameof(GetMaterializedViewColumnsQueryResult.Collation) }"",
    atc.VIRTUAL_COLUMN as ""{ nameof(GetMaterializedViewColumnsQueryResult.IsComputed) }""
from SYS.ALL_TAB_COLS atc
where OWNER = :{ nameof(GetMaterializedViewColumnsQuery.SchemaName) } and TABLE_NAME = :{ nameof(GetMaterializedViewColumnsQuery.ViewName) }
order by atc.COLUMN_ID";

        /// <summary>
        /// Retrieves the names all of the not-null constrained columns in a given materialized view.
        /// </summary>
        /// <param name="viewName">A materialized view name.</param>
        /// <param name="columnNames">The column names for the given materialized view.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of not-null constrained column names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> or <paramref name="columnNames"/> are <c>null</c>.</exception>
        protected Task<IEnumerable<string>> GetNotNullConstrainedColumnsAsync(Identifier viewName, IEnumerable<string> columnNames, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (columnNames == null)
                throw new ArgumentNullException(nameof(columnNames));

            return GetNotNullConstrainedColumnsAsyncCore(viewName, columnNames, cancellationToken);
        }

        private async Task<IEnumerable<string>> GetNotNullConstrainedColumnsAsyncCore(Identifier viewName, IEnumerable<string> columnNames, CancellationToken cancellationToken)
        {
            var checks = await DbConnection.QueryAsync<GetMaterializedViewChecksQueryResult>(
                ChecksQuery,
                new GetMaterializedViewChecksQuery { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (checks.Empty())
                return Array.Empty<string>();

            var columnNotNullConstraints = columnNames
                .Select(name => new KeyValuePair<string, string>(GenerateNotNullDefinition(name), name))
                .ToReadOnlyDictionary();

            return checks
                .Where(c => c.Definition != null && columnNotNullConstraints.ContainsKey(c.Definition) && string.Equals(c.EnabledStatus, EnabledValue, StringComparison.Ordinal))
                .Select(c => columnNotNullConstraints[c.Definition!])
                .ToList();
        }

        /// <summary>
        /// A SQL query that retrieves check constraint information for a view.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ChecksQuery => ChecksQuerySql;

        private const string ChecksQuerySql = @$"
select
    CONSTRAINT_NAME as ""{ nameof(GetMaterializedViewChecksQueryResult.ConstraintName) }"",
    SEARCH_CONDITION as ""{ nameof(GetMaterializedViewChecksQueryResult.Definition) }"",
    STATUS as ""{ nameof(GetMaterializedViewChecksQueryResult.EnabledStatus) }""
from SYS.ALL_CONSTRAINTS
where OWNER = :{ nameof(GetMaterializedViewChecksQuery.SchemaName) } and TABLE_NAME = :{ nameof(GetMaterializedViewChecksQuery.ViewName) } and CONSTRAINT_TYPE = 'C'";

        /// <summary>
        /// Creates a not null constraint definition, used to determine whether a constraint is a <c>NOT NULL</c> constraint.
        /// </summary>
        /// <param name="columnName">A column name.</param>
        /// <returns>A <c>NOT NULL</c> constraint definition for the given column.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="columnName"/> is <c>null</c>, empty or whitespace.</exception>
        protected string GenerateNotNullDefinition(string columnName)
        {
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            return _notNullDefinitions.GetOrAdd(columnName, static colName => "\"" + colName + "\" IS NOT NULL");
        }

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

        private readonly ConcurrentDictionary<string, string> _notNullDefinitions = new();

        private const string EnabledValue = "ENABLED";
    }
}
