using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

        public override IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return database.Tables.SelectMany(t => AnalyseTable(database.Dialect, t)).ToList();
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IDatabaseDialect dialect, IRelationalDatabaseTable table)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var nullableColumns = table.Columns.Where(c => c.IsNullable).ToList();
            if (nullableColumns.Count == 0)
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
