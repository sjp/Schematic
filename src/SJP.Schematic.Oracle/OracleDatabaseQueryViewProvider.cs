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

namespace SJP.Schematic.Oracle
{
    /// <summary>
    /// A query view provider for Oracle.
    /// </summary>
    /// <seealso cref="IDatabaseViewProvider" />
    public class OracleDatabaseQueryViewProvider : IDatabaseViewProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleDatabaseQueryViewProvider"/> class.
        /// </summary>
        /// <param name="connection">A schematic connection.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
        public OracleDatabaseQueryViewProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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
        /// Gets all database views.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database views.</returns>
        public virtual async IAsyncEnumerable<IDatabaseView> GetAllViews([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResult = await DbConnection.QueryAsync<QualifiedName>(ViewsQuery, cancellationToken).ConfigureAwait(false);
            var viewNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .Select(QualifyViewName);

            foreach (var viewName in viewNames)
                yield return await LoadViewAsyncCore(viewName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// A SQL query that retrieves the names of views available in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ViewsQuery => ViewsQuerySql;

        private static readonly string ViewsQuerySql = @$"
select
    v.OWNER as ""{ nameof(QualifiedName.SchemaName) }"",
    v.VIEW_NAME as ""{ nameof(QualifiedName.ObjectName) }""
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by v.OWNER, v.VIEW_NAME";

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
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedViewNameStrict(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            var qualifiedViewName = DbConnection.QueryFirstOrNone<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = candidateViewName.Schema, ViewName = candidateViewName.LocalName },
                cancellationToken
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ObjectName));
        }

        /// <summary>
        /// A SQL query that retrieves the resolved name of a view in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private static readonly string ViewNameQuerySql = @$"
select v.OWNER as ""{ nameof(QualifiedName.SchemaName) }"", v.VIEW_NAME as ""{ nameof(QualifiedName.ObjectName) }""
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
where v.OWNER = :SchemaName and v.VIEW_NAME = :ViewName and o.ORACLE_MAINTAINED <> 'Y'";

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

            return new DatabaseView(viewName, definition, columns);
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
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                cancellationToken
            );
        }

        /// <summary>
        /// A SQL query that retrieves the definition of a view.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string DefinitionQuery => DefinitionQuerySql;

        private const string DefinitionQuerySql = @"
select TEXT
from SYS.ALL_VIEWS
where OWNER = :SchemaName and VIEW_NAME = :ViewName";

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
            var query = await DbConnection.QueryAsync<ColumnData>(
                ColumnsQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var columnNames = query
                .Where(row => row.ColumnName != null)
                .Select(row => row.ColumnName!)
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

        private static readonly string ColumnsQuerySql = @$"
select
    atc.COLUMN_NAME as ""{ nameof(ColumnData.ColumnName) }"",
    atc.DATA_TYPE_OWNER as ""{ nameof(ColumnData.ColumnTypeSchema) }"",
    atc.DATA_TYPE as ""{ nameof(ColumnData.ColumnTypeName) }"",
    atc.DATA_LENGTH as ""{ nameof(ColumnData.DataLength) }"",
    atc.DATA_PRECISION as ""{ nameof(ColumnData.Precision) }"",
    atc.DATA_SCALE as ""{ nameof(ColumnData.Scale) }"",
    atc.DATA_DEFAULT as ""{ nameof(ColumnData.DefaultValue) }"",
    atc.CHAR_LENGTH as ""{ nameof(ColumnData.CharacterLength) }"",
    atc.CHARACTER_SET_NAME as ""{ nameof(ColumnData.Collation) }"",
    atc.VIRTUAL_COLUMN as ""{ nameof(ColumnData.IsComputed) }""
from SYS.ALL_TAB_COLS atc
where OWNER = :SchemaName and TABLE_NAME = :ViewName
order by atc.COLUMN_ID";

        /// <summary>
        /// Retrieves the names all of the not-null constrained columns in a given view.
        /// </summary>
        /// <param name="viewName">A view name.</param>
        /// <param name="columnNames">The column names for the given view.</param>
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
            var checks = await DbConnection.QueryAsync<CheckConstraintData>(
                ChecksQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (checks.Empty())
                return Array.Empty<string>();

            var columnNotNullConstraints = columnNames
                .Select(name => new KeyValuePair<string, string>(GenerateNotNullDefinition(name), name))
                .ToDictionary();

            return checks
                .Where(c => c.Definition != null && columnNotNullConstraints.ContainsKey(c.Definition) && c.EnabledStatus == EnabledValue)
                .Select(c => columnNotNullConstraints[c.Definition!])
                .ToList();
        }

        /// <summary>
        /// A SQL query that retrieves check constraint information for a view.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ChecksQuery => ChecksQuerySql;

        private static readonly string ChecksQuerySql = @$"
select
    CONSTRAINT_NAME as ""{ nameof(CheckConstraintData.ConstraintName) }"",
    SEARCH_CONDITION as ""{ nameof(CheckConstraintData.Definition) }"",
    STATUS as ""{ nameof(CheckConstraintData.EnabledStatus) }""
from SYS.ALL_CONSTRAINTS
where OWNER = :SchemaName and TABLE_NAME = :ViewName and CONSTRAINT_TYPE = 'C'";

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

            return _notNullDefinitions.GetOrAdd(columnName, colName => "\"" + colName + "\" IS NOT NULL");
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

        private readonly ConcurrentDictionary<string, string> _notNullDefinitions = new ConcurrentDictionary<string, string>();

        private const string EnabledValue = "ENABLED";
    }
}
