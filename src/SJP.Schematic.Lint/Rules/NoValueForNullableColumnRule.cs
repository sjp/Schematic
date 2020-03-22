using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class NoValueForNullableColumnRule : Rule, ITableRule
    {
        public NoValueForNullableColumnRule(ISchematicConnection connection, RuleLevel level)
            : base(RuleTitle, level)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            _fromQuerySuffixAsync = new AsyncLazy<string>(GetFromQuerySuffixAsync);
        }

        protected ISchematicConnection Connection { get; }

        protected IDbConnection DbConnection => Connection.DbConnection;

        protected IDatabaseDialect Dialect => Connection.Dialect;

        public IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return AnalyseTablesCore(tables, cancellationToken);
        }

        private async IAsyncEnumerable<IRuleMessage> AnalyseTablesCore(IEnumerable<IRelationalDatabaseTable> tables, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var table in tables)
            {
                var messages = await AnalyseTableAsync(table, cancellationToken).ConfigureAwait(false);
                foreach (var message in messages)
                    yield return message;
            }
        }

        protected Task<IEnumerable<IRuleMessage>> AnalyseTableAsync(IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return AnalyseTableAsyncCore(table, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseTableAsyncCore(IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            var nullableColumns = table.Columns.Where(c => c.IsNullable).ToList();
            if (nullableColumns.Empty())
                return Array.Empty<IRuleMessage>();

            var tableHasRows = await TableHasRowsAsync(table, cancellationToken).ConfigureAwait(false);
            if (!tableHasRows)
                return Array.Empty<IRuleMessage>();

            var result = new List<IRuleMessage>();

            foreach (var nullableColumn in nullableColumns)
            {
                var hasValue = await NullableColumnHasValueAsync(table, nullableColumn, cancellationToken).ConfigureAwait(false);
                if (hasValue)
                    continue;

                var message = BuildMessage(table.Name, nullableColumn.Name.LocalName);
                result.Add(message);
            }

            return result;
        }

        protected Task<bool> TableHasRowsAsync(IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return TableHasRowsAsyncCore(table, cancellationToken);
        }

        private async Task<bool> TableHasRowsAsyncCore(IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            var sql = await GetTableHasRowsQueryAsync(table.Name).ConfigureAwait(false);
            return await DbConnection.ExecuteScalarAsync<bool>(sql, cancellationToken).ConfigureAwait(false);
        }

        protected Task<bool> NullableColumnHasValueAsync(IRelationalDatabaseTable table, IDatabaseColumn column,
            CancellationToken cancellationToken)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return NullableColumnHasValueAsyncCore(table, column, cancellationToken);
        }

        private async Task<bool> NullableColumnHasValueAsyncCore(IRelationalDatabaseTable table, IDatabaseColumn column,
            CancellationToken cancellationToken)
        {
            var sql = await GetNullableColumnHasValueQueryAsync(table.Name, column.Name).ConfigureAwait(false);
            return await DbConnection.ExecuteScalarAsync<bool>(sql, cancellationToken).ConfigureAwait(false);
        }

        protected virtual IRuleMessage BuildMessage(Identifier tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var messageText = $"The table '{ tableName }' has a nullable column '{ columnName }' whose values are always null. Consider removing the column.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        private Task<string> GetTableHasRowsQueryAsync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetTableHasRowsQueryAsyncCore(tableName);
        }

        private async Task<string> GetTableHasRowsQueryAsyncCore(Identifier tableName)
        {
            var quotedTableName = Dialect.QuoteName(Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName));
            var filterSql = "select 1 as dummy_col from " + quotedTableName;
            var sql = $"select case when exists ({ filterSql }) then 1 else 0 end as dummy";

            var suffix = await _fromQuerySuffixAsync.ConfigureAwait(false);
            return suffix.IsNullOrWhiteSpace()
                ? sql
                : sql + " from " + suffix;
        }

        private Task<string> GetNullableColumnHasValueQueryAsync(Identifier tableName, Identifier columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            return GetNullableColumnHasValueQueryCore(tableName, columnName);
        }

        private async Task<string> GetNullableColumnHasValueQueryCore(Identifier tableName, Identifier columnName)
        {
            var quotedTableName = Dialect.QuoteName(Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName));
            var quotedColumnName = Dialect.QuoteIdentifier(columnName.LocalName);
            var filterSql = $"select * from { quotedTableName } where { quotedColumnName } is not null";
            var sql = $"select case when exists ({ filterSql }) then 1 else 0 end as dummy";

            var suffix = await _fromQuerySuffixAsync.ConfigureAwait(false);
            return suffix.IsNullOrWhiteSpace()
                ? sql
                : sql + " from " + suffix;
        }

        private async Task<string> GetFromQuerySuffixAsync()
        {
            try
            {
                _ = await DbConnection.ExecuteScalarAsync<bool>(TestQueryNoTable, CancellationToken.None).ConfigureAwait(false);
                return string.Empty;
            }
            catch
            {
                // Deliberately ignoring because we are testing functionality
            }

            try
            {
                _ = await DbConnection.ExecuteScalarAsync<bool>(TestQueryFromSysDual, CancellationToken.None).ConfigureAwait(false);
                return "SYS.DUAL";
            }
            catch
            {
                // Deliberately ignoring because we are testing functionality
            }

            _ = await DbConnection.ExecuteScalarAsync<bool>(TestQueryFromDual, CancellationToken.None).ConfigureAwait(false);
            return "DUAL";
        }

        protected static string RuleTitle { get; } = "No not-null values exist for a nullable column.";

        private const string TestQueryNoTable = "select 1 as dummy";
        private const string TestQueryFromDual = "select 1 as dummy from DUAL";
        private const string TestQueryFromSysDual = "select 1 as dummy from SYS.DUAL";

        private readonly AsyncLazy<string> _fromQuerySuffixAsync;
    }
}
