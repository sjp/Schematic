using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Data.Sqlite;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite
{
    public class SqliteDatabaseViewProvider : IDatabaseViewProvider
    {
        public SqliteDatabaseViewProvider(IDbConnection connection, ISqliteConnectionPragma pragma, IDatabaseDialect dialect, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            ConnectionPragma = pragma ?? throw new ArgumentNullException(nameof(pragma));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        protected IDbConnection Connection { get; }

        protected ISqliteConnectionPragma ConnectionPragma { get; }

        protected IDatabaseDialect Dialect { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        public async Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default(CancellationToken))
        {
            var dbNamesQuery = await ConnectionPragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesQuery.OrderBy(d => d.seq).Select(l => l.name).ToList();

            var qualifiedViewNames = new List<Identifier>();

            foreach (var dbName in dbNames)
            {
                var sql = ViewsQuery(dbName);
                var queryResult = await Connection.QueryAsync<string>(sql, cancellationToken).ConfigureAwait(false);
                var viewNames = queryResult
                    .Where(name => !IsReservedTableName(name))
                    .Select(name => Identifier.CreateQualifiedIdentifier(dbName, name));

                qualifiedViewNames.AddRange(viewNames);
            }

            var views = await qualifiedViewNames
                .Select(name => LoadView(name, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return views.ToList();
        }

        protected virtual string ViewsQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return $"select name from { Dialect.QuoteIdentifier(schemaName) }.sqlite_master where type = 'view' order by name";
        }

        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
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

            var dbNamesResult = await ConnectionPragma.DatabaseListAsync().ConfigureAwait(false);
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
                var viewLocalName = await Connection.ExecuteScalarAsync<string>(
                    sql,
                    new { ViewName = viewName.LocalName },
                    cancellationToken
                ).ConfigureAwait(false);

                if (viewLocalName != null)
                {
                    var dbList = await ConnectionPragma.DatabaseListAsync().ConfigureAwait(false);
                    var viewSchemaName = dbList
                        .OrderBy(s => s.seq)
                        .Select(s => s.name)
                        .FirstOrDefault(s => string.Equals(s, viewName.Schema, StringComparison.OrdinalIgnoreCase));
                    if (viewSchemaName == null)
                        throw new InvalidOperationException("Unable to find a database matching the given schema name: " + viewName.Schema);

                    return Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(viewSchemaName, viewLocalName));
                }
            }

            var dbNamesResult = await ConnectionPragma.DatabaseListAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var sql = ViewNameQuery(dbName);
                var viewLocalName = await Connection.ExecuteScalarAsync<string>(
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

        protected virtual OptionAsync<IDatabaseView> LoadView(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            return LoadViewAsyncCore(candidateViewName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseView>> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var candidateViewName = QualifyViewName(viewName);
            var resolvedViewNameOption = GetResolvedViewName(candidateViewName, cancellationToken);
            var resolvedViewNameOptionIsNone = await resolvedViewNameOption.IsNone.ConfigureAwait(false);
            if (resolvedViewNameOptionIsNone)
                return Option<IDatabaseView>.None;

            var resolvedViewName = await resolvedViewNameOption.UnwrapSomeAsync().ConfigureAwait(false);
            var databasePragma = new DatabasePragma(Dialect, Connection, resolvedViewName.Schema);

            var columnsTask = LoadColumnsAsync(databasePragma, resolvedViewName, cancellationToken);
            var definitionTask = LoadDefinitionAsync(resolvedViewName, cancellationToken);
            await Task.WhenAll(columnsTask, definitionTask).ConfigureAwait(false);

            var columns = columnsTask.Result;
            var definition = definitionTask.Result;

            var view = new DatabaseView(resolvedViewName, definition, columns);
            return Option<IDatabaseView>.Some(view);
        }

        protected virtual Task<string> LoadDefinitionAsync(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var sql = DefinitionQuery(viewName.Schema);
            return Connection.ExecuteScalarAsync<string>(
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
                tableInfos = await pragma.TableInfoAsync(viewName).ConfigureAwait(false);
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
                var columnTypeName = tableInfo.type;
                if (columnTypeName.IsNullOrWhiteSpace())
                    columnTypeName = await GetTypeofColumnAsync(viewName, tableInfo.name, cancellationToken).ConfigureAwait(false);

                var affinity = AffinityParser.ParseTypeName(columnTypeName);
                var columnType = new SqliteColumnType(affinity);
                var defaultValue = !tableInfo.dflt_value.IsNullOrWhiteSpace()
                    ? Option<string>.Some(tableInfo.dflt_value)
                    : Option<string>.None;

                var column = new DatabaseColumn(tableInfo.name, columnType, !tableInfo.notnull, defaultValue, Option<IAutoIncrement>.None);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual Task<string> GetTypeofColumnAsync(Identifier viewName, Identifier columnName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            var sql = GetTypeofQuery(viewName, columnName.LocalName);
            return Connection.ExecuteScalarAsync<string>(sql, cancellationToken);
        }

        protected virtual string GetTypeofQuery(Identifier viewName, string columnName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            return $"select typeof({ Dialect.QuoteName(columnName) }) from { Dialect.QuoteName(viewName) } limit 1";
        }

        protected Identifier QualifyViewName(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var schema = viewName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(schema, viewName.LocalName);
        }

        protected static bool IsReservedTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return tableName.LocalName.StartsWith("sqlite_", StringComparison.OrdinalIgnoreCase);
        }

        private static readonly SqliteTypeAffinityParser AffinityParser = new SqliteTypeAffinityParser();

        private const int SqliteError = 1;
    }
}
