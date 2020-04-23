using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    /// <summary>
    /// A linting rule which reports when no non-null values exist for a nullable column in a table.
    /// </summary>
    /// <seealso cref="Rule"/>
    /// <seealso cref="ITableRule"/>
    public class NoValueForNullableColumnRule : Rule, ITableRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoValueForNullableColumnRule"/> class.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="level">The reporting level.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public NoValueForNullableColumnRule(ISchematicConnection connection, RuleLevel level)
            : base(RuleId, RuleTitle, level)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            _fromQuerySuffixAsync = new AsyncLazy<string>(GetFromQuerySuffixAsync);
        }

        /// <summary>
        /// A database connection, qualified with a dialect.
        /// </summary>
        /// <value>The connection.</value>
        protected ISchematicConnection Connection { get; }

        /// <summary>
        /// A database connection factory.
        /// </summary>
        /// <value>The database connection factory.</value>
        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        /// <summary>
        /// A database dialect.
        /// </summary>
        /// <value>The dialect associated with <see cref="DbConnection"/>.</value>
        protected IDatabaseDialect Dialect => Connection.Dialect;

        /// <summary>
        /// Analyses database tables. Reports messages when no non-null values exist for a nullable column in a table.
        /// </summary>
        /// <param name="tables">A set of database tables.</param>
        /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
        /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <c>null</c>.</exception>
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

        /// <summary>
        /// Analyses a database table. Reports messages when no non-null values exist for a nullable column in a table.
        /// </summary>
        /// <param name="table">A database table.</param>
        /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
        /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
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

        /// <summary>
        /// Determines whether a table has any rows present.
        /// </summary>
        /// <param name="table">A database table.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns><c>true</c> if the table has any rows; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
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

        /// <summary>
        /// Determines whether a nullable column has any non-null values.
        /// </summary>
        /// <param name="table">A database table.</param>
        /// <param name="column">A column from the table provided by <paramref name="table"/>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns><c>true</c> if the column has any non-null values; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> or <paramref name="column"/> is <c>null</c>.</exception>
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
        /// <summary>
        /// Builds the message used for reporting.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">A name of the nullable column.</param>
        /// <returns>A formatted linting message.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>. Also thrown when <paramref name="columnName"/> is <c>null</c>, empty or whitespace.</exception>
        protected virtual IRuleMessage BuildMessage(Identifier tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var messageText = $"The table '{ tableName }' has a nullable column '{ columnName }' whose values are always null. Consider removing the column.";
            return new RuleMessage(RuleId, RuleTitle, Level, messageText);
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

        /// <summary>
        /// The rule identifier.
        /// </summary>
        /// <value>A rule identifier.</value>
        protected static string RuleId { get; } = "SCHEMATIC0014";

        /// <summary>
        /// Gets the rule title.
        /// </summary>
        /// <value>The rule title.</value>
        protected static string RuleTitle { get; } = "No not-null values exist for a nullable column.";

        private const string TestQueryNoTable = "select 1 as dummy";
        private const string TestQueryFromDual = "select 1 as dummy from DUAL";
        private const string TestQueryFromSysDual = "select 1 as dummy from SYS.DUAL";

        private readonly AsyncLazy<string> _fromQuerySuffixAsync;
    }
}
