using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class NoValueForNullableColumnRule : Rule
    {
        public NoValueForNullableColumnRule(IDbConnection connection, RuleLevel level)
            : base(RuleTitle, level)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        protected IDbConnection Connection { get; }

        public override Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsync(IRelationalDatabase database, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return AnalyseDatabaseAsyncCore(database, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsyncCore(IRelationalDatabase database, CancellationToken cancellationToken)
        {
            var tables = await database.GetAllTables(cancellationToken).ConfigureAwait(false);
            if (tables.Empty())
                return Array.Empty<IRuleMessage>();

            var result = new List<IRuleMessage>();
            foreach (var table in tables)
            {
                var messages = await AnalyseTableAsync(database.Dialect, table, cancellationToken).ConfigureAwait(false);
                result.AddRange(messages);
            }
            return result;
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IDatabaseDialect dialect, IRelationalDatabaseTable table)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var nullableColumns = table.Columns.Where(c => c.IsNullable).ToList();
            if (nullableColumns.Empty())
                return Array.Empty<IRuleMessage>();

            var tableRowCount = GetRowCount(dialect, table);
            if (tableRowCount == 0)
                return Array.Empty<IRuleMessage>();

            var result = new List<IRuleMessage>();

            foreach (var nullableColumn in nullableColumns)
            {
                var nullableRowCount = GetColumnNullableRowCount(dialect, table, nullableColumn);
                if (nullableRowCount != tableRowCount)
                    continue;

                var message = BuildMessage(table.Name, nullableColumn.Name.LocalName);
                result.Add(message);
            }

            return result;
        }

        protected Task<IEnumerable<IRuleMessage>> AnalyseTableAsync(IDatabaseDialect dialect, IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return AnalyseTableAsyncCore(dialect, table, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseTableAsyncCore(IDatabaseDialect dialect, IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            var nullableColumns = table.Columns.Where(c => c.IsNullable).ToList();
            if (nullableColumns.Empty())
                return Array.Empty<IRuleMessage>();

            var tableRowCount = await GetRowCountAsync(dialect, table, cancellationToken).ConfigureAwait(false);
            if (tableRowCount == 0)
                return Array.Empty<IRuleMessage>();

            var result = new List<IRuleMessage>();

            foreach (var nullableColumn in nullableColumns)
            {
                var nullableRowCount = await GetColumnNullableRowCountAsync(dialect, table, nullableColumn, cancellationToken).ConfigureAwait(false);
                if (nullableRowCount != tableRowCount)
                    continue;

                var message = BuildMessage(table.Name, nullableColumn.Name.LocalName);
                result.Add(message);
            }

            return result;
        }

        protected long GetRowCount(IDatabaseDialect dialect, IRelationalDatabaseTable table)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var sql = $"select count(*) from { dialect.QuoteName(table.Name) }";
            return Connection.ExecuteScalar<long>(sql);
        }

        protected long GetColumnNullableRowCount(IDatabaseDialect dialect, IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var sql = $"select count(*) from { dialect.QuoteName(table.Name) } where { dialect.QuoteIdentifier(column.Name.LocalName) } is null";
            return Connection.ExecuteScalar<long>(sql);
        }

        protected Task<long> GetRowCountAsync(IDatabaseDialect dialect, IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var sql = $"select count(*) from { dialect.QuoteName(table.Name) }";
            return Connection.ExecuteScalarAsync<long>(sql, cancellationToken);
        }

        protected Task<long> GetColumnNullableRowCountAsync(IDatabaseDialect dialect, IRelationalDatabaseTable table, IDatabaseColumn column, CancellationToken cancellationToken)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var sql = $"select count(*) from { dialect.QuoteName(table.Name) } where { dialect.QuoteIdentifier(column.Name.LocalName) } is null";
            return Connection.ExecuteScalarAsync<long>(sql, cancellationToken);
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

        protected static string RuleTitle { get; } = "No not-null values exist for a nullable column.";
    }
}
