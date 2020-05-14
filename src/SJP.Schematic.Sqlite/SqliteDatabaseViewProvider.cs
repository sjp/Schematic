using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Data.Sqlite;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite
{
    /// <summary>
    /// A view provider for SQLite.
    /// </summary>
    /// <seealso cref="IDatabaseViewProvider" />
    public class SqliteDatabaseViewProvider : IDatabaseViewProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDatabaseViewProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="pragma">A connection pragma for the SQLite connection.</param>
        /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="pragma"/> or <paramref name="identifierDefaults"/> are <c>null</c>.</exception>
        public SqliteDatabaseViewProvider(ISchematicConnection connection, ISqliteConnectionPragma pragma, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            ConnectionPragma = pragma ?? throw new ArgumentNullException(nameof(pragma));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        /// <summary>
        /// A database connection that is specific to a given SQLite database.
        /// </summary>
        /// <value>A database connection.</value>
        protected ISchematicConnection Connection { get; }

        protected ISqliteConnectionPragma ConnectionPragma { get; }

        /// <summary>
        /// Identifier defaults for the associated database.
        /// </summary>
        /// <value>Identifier defaults.</value>
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
            var dbNamesQuery = await ConnectionPragma.DatabaseListAsync(cancellationToken).ConfigureAwait(false);
            var dbNames = dbNamesQuery
                .OrderBy(d => d.seq)
                .Select(d => d.name)
                .ToList();

            var qualifiedViewNames = new List<Identifier>();

            foreach (var dbName in dbNames)
            {
                var sql = ViewsQuery(dbName);
                var queryResult = await DbConnection.QueryAsync<string>(sql, cancellationToken).ConfigureAwait(false);
                var viewNames = queryResult
                    .Where(name => !IsReservedTableName(name))
                    .Select(name => Identifier.CreateQualifiedIdentifier(dbName, name));

                qualifiedViewNames.AddRange(viewNames);
            }

            var orderedViewNames = qualifiedViewNames
                .OrderBy(v => v.Schema)
                .ThenBy(v => v.LocalName);

            foreach (var viewName in orderedViewNames)
                yield return await LoadViewAsyncCore(viewName, cancellationToken).ConfigureAwait(false);
        }

        protected virtual string ViewsQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return $"select name from { Dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'view' order by name";
        }

        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return GetViewAsyncCore(viewName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseView>> GetViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName.Schema != null)
            {
                return await LoadView(viewName, cancellationToken)
                    .ToOption()
                    .ConfigureAwait(false);
            }

            var dbNamesResult = await ConnectionPragma.DatabaseListAsync(cancellationToken).ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedViewName = Identifier.CreateQualifiedIdentifier(dbName, viewName.LocalName);
                var view = LoadView(qualifiedViewName, cancellationToken);

                var viewIsSome = await view.IsSome.ConfigureAwait(false);
                if (viewIsSome)
                    return await view.ToOption().ConfigureAwait(false);
            }

            return Option<IDatabaseView>.None;
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

            return GetResolvedViewNameAsyncCore(viewName, cancellationToken).ToAsync();
        }

        private async Task<Option<Identifier>> GetResolvedViewNameAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName.Schema != null)
            {
                var sql = ViewNameQuery(viewName.Schema);
                var viewLocalName = await DbConnection.ExecuteScalarAsync<string>(
                    sql,
                    new { ViewName = viewName.LocalName },
                    cancellationToken
                ).ConfigureAwait(false);

                if (viewLocalName != null)
                {
                    var dbList = await ConnectionPragma.DatabaseListAsync(cancellationToken).ConfigureAwait(false);
                    var viewSchemaName = dbList
                        .OrderBy(s => s.seq)
                        .Select(s => s.name)
                        .FirstOrDefault(s => string.Equals(s, viewName.Schema, StringComparison.OrdinalIgnoreCase));
                    if (viewSchemaName == null)
                        throw new InvalidOperationException("Unable to find a database matching the given schema name: " + viewName.Schema);

                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(viewSchemaName, viewLocalName));
                }
            }

            var dbNamesResult = await ConnectionPragma.DatabaseListAsync(cancellationToken).ConfigureAwait(false);
            var dbNames = dbNamesResult
                .OrderBy(l => l.seq)
                .Select(l => l.name)
                .ToList();
            foreach (var dbName in dbNames)
            {
                var sql = ViewNameQuery(dbName);
                var viewLocalName = await DbConnection.ExecuteScalarAsync<string>(
                    sql,
                    new { ViewName = viewName.LocalName },
                    cancellationToken
                ).ConfigureAwait(false);

                if (viewLocalName != null)
                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(dbName, viewLocalName));
            }

            return Option<Identifier>.None;
        }

        protected virtual string ViewNameQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return $"select name from { Dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'view' and lower(name) = lower(@ViewName)";
        }

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
            var databasePragma = GetDatabasePragma(viewName.Schema!);

            var columnsTask = LoadColumnsAsync(databasePragma, viewName, cancellationToken);
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

            var sql = DefinitionQuery(viewName.Schema!);
            return DbConnection.ExecuteScalarAsync<string>(
                sql,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                cancellationToken
            );
        }

        protected virtual string DefinitionQuery(string schema)
        {
            if (schema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schema));

            return $"select sql from { Dialect.QuoteIdentifier(schema) }.sqlite_master where type = 'view' and tbl_name = @ViewName";
        }

        /// <summary>
        /// Retrieves the columns for a given view.
        /// </summary>
        /// <param name="pragma">A schema-specific pragma accessor.</param>
        /// <param name="viewName">A view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An ordered collection of columns.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected virtual Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(ISqliteDatabasePragma pragma, Identifier viewName, CancellationToken cancellationToken)
        {
            if (pragma == null)
                throw new ArgumentNullException(nameof(pragma));
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadColumnsAsyncCore(pragma, viewName, cancellationToken);
        }

        private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(ISqliteDatabasePragma pragma, Identifier viewName, CancellationToken cancellationToken)
        {
            IEnumerable<Pragma.Query.pragma_table_info> tableInfos;
            try
            {
                // When the view is invalid, this may throw an exception so we catch it.
                // This does mean that we are in a partial state, but if the definition is corrected
                // and the view is queried again then we'll end up with something correct.
                tableInfos = await pragma.TableInfoAsync(viewName, cancellationToken).ConfigureAwait(false);
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == SqliteError)
            {
                return Array.Empty<IDatabaseColumn>();
            }

            if (tableInfos.Empty())
                return Array.Empty<IDatabaseColumn>();

            var result = new List<IDatabaseColumn>();
            foreach (var tableInfo in tableInfos)
            {
                if (tableInfo.name == null)
                    continue;

                var columnName = tableInfo.name;

                var columnTypeName = tableInfo.type;
                if (columnTypeName.IsNullOrWhiteSpace())
                    columnTypeName = await GetTypeofColumnAsync(viewName, columnName, cancellationToken).ConfigureAwait(false);

                var affinity = AffinityParser.ParseTypeName(columnTypeName);
                var columnType = new SqliteColumnType(affinity);
                var defaultValue = !tableInfo.dflt_value.IsNullOrWhiteSpace()
                    ? Option<string>.Some(tableInfo.dflt_value)
                    : Option<string>.None;

                var column = new DatabaseColumn(columnName, columnType, !tableInfo.notnull, defaultValue, Option<IAutoIncrement>.None);
                result.Add(column);
            }

            return result;
        }

        protected virtual Task<string> GetTypeofColumnAsync(Identifier viewName, Identifier columnName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            var sql = GetTypeofQuery(viewName, columnName.LocalName);
            return DbConnection.ExecuteScalarAsync<string>(sql, cancellationToken);
        }

        protected virtual string GetTypeofQuery(Identifier viewName, string columnName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            return $"select typeof({ Dialect.QuoteName(columnName) }) from { Dialect.QuoteName(viewName) } limit 1";
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
            return Identifier.CreateQualifiedIdentifier(schema, viewName.LocalName);
        }

        protected virtual ISqliteDatabasePragma GetDatabasePragma(string schema)
        {
            if (schema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schema));

            var loader = _dbPragmaCache.GetOrAdd(schema, new Lazy<ISqliteDatabasePragma>(() => new DatabasePragma(Connection, schema)));
            return loader.Value;
        }

        protected static bool IsReservedTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return tableName.LocalName.StartsWith("sqlite_", StringComparison.OrdinalIgnoreCase);
        }

        private readonly ConcurrentDictionary<string, Lazy<ISqliteDatabasePragma>> _dbPragmaCache = new ConcurrentDictionary<string, Lazy<ISqliteDatabasePragma>>();
        private static readonly SqliteTypeAffinityParser AffinityParser = new SqliteTypeAffinityParser();

        private const int SqliteError = 1;
    }
}
