using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Analysis.Rules
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

            return database.Tables.SelectMany(AnalyseTable).ToList();
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var nullableColumns = table.Columns.Where(c => c.IsNullable).ToList();
            if (nullableColumns.Count == 0)
                return Enumerable.Empty<IRuleMessage>();

            var dialect = table.Database.Dialect;
            var tableRowCount = GetRowCount(dialect, table);
            if (tableRowCount == 0)
                return Enumerable.Empty<IRuleMessage>();

            var result = new List<IRuleMessage>();

            foreach (var nullableColumn in nullableColumns)
            {
                var nullableRowCount = GetColumnNullableRowCount(dialect, table, nullableColumn);
                if (nullableRowCount != tableRowCount)
                    continue;

                var messageText = $"The table '{ table.Name }' has a nullable column '{ nullableColumn.Name.LocalName }' whose values are always null. Consider removing the column.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);

                result.Add(ruleMessage);
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

        private const string RuleTitle = "No not-null values exist for a nullable column.";
    }
}
