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
    public class NoValueForNullableColumnRule : Rule, ITableRule
    {
        public NoValueForNullableColumnRule(IDbConnection connection, IDatabaseDialect dialect, RuleLevel level)
            : base(RuleTitle, level)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        protected IDbConnection Connection { get; }

        protected IDatabaseDialect Dialect { get; }

        public IEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return tables.SelectMany(AnalyseTable).ToList();
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return AnalyseTablesAsyncCore(tables, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseTablesAsyncCore(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tables.Empty())
                return Array.Empty<IRuleMessage>();

            var result = new List<IRuleMessage>();

            foreach (var table in tables)
            {
                var messages = await AnalyseTableAsync(table, cancellationToken).ConfigureAwait(false);
                result.AddRange(messages);
            }

            return result;
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

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var nullableColumns = table.Columns.Where(c => c.IsNullable).ToList();
            if (nullableColumns.Empty())
                return Array.Empty<IRuleMessage>();

            var tableHasRows = TableHasRows(table);
            if (!tableHasRows)
                return Array.Empty<IRuleMessage>();

            var result = new List<IRuleMessage>();

            foreach (var nullableColumn in nullableColumns)
            {
                if (NullableColumnHasValue(table, nullableColumn))
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

            var tableName = Identifier.CreateQualifiedIdentifier(table.Name.Schema, table.Name.LocalName);
            var filterSql = $"select 1 as dummy_col from { Dialect.QuoteName(tableName) }";
            var sql = $"select case when exists ({ filterSql }) then 1 else 0 end as dummy";
            return Connection.ExecuteScalarAsync<bool>(sql, cancellationToken);
        }

        protected Task<bool> NullableColumnHasValueAsync(IRelationalDatabaseTable table, IDatabaseColumn column,
            CancellationToken cancellationToken)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var tableName = Identifier.CreateQualifiedIdentifier(table.Name.Schema, table.Name.LocalName);
            var filterSql = $"select * from { Dialect.QuoteName(tableName) } where { Dialect.QuoteIdentifier(column.Name.LocalName) } is not null";
            var sql = $"select case when exists ({ filterSql }) then 1 else 0 end as dummy";
            return Connection.ExecuteScalarAsync<bool>(sql, cancellationToken);
        }

        protected bool TableHasRows(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var tableName = Identifier.CreateQualifiedIdentifier(table.Name.Schema, table.Name.LocalName);
            var filterSql = $"select 1 as dummy_col from { Dialect.QuoteName(tableName) }";
            var sql = $"select case when exists ({ filterSql }) then 1 else 0 end as dummy";
            return Connection.ExecuteScalar<bool>(sql);
        }

        protected bool NullableColumnHasValue(IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var tableName = Identifier.CreateQualifiedIdentifier(table.Name.Schema, table.Name.LocalName);
            var filterSql = $"select * from { Dialect.QuoteName(tableName) } where { Dialect.QuoteIdentifier(column.Name.LocalName) } is not null";
            var sql = $"select case when exists ({ filterSql }) then 1 else 0 end as dummy";
            return Connection.ExecuteScalar<bool>(sql);
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
