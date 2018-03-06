using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class NumericValuesForTextRule : Rule
    {
        public NumericValuesForTextRule(IDbConnection connection, RuleLevel level)
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

            var textColumns = table.Columns.Where(c => TextColumnTypes.Contains(c.Type.DataType)).ToList();
            if (textColumns.Count == 0)
                return Enumerable.Empty<IRuleMessage>();

            var dialect = table.Database.Dialect;
            var tableRowCount = GetRowCount(dialect, table);
            if (tableRowCount == 0)
                return Enumerable.Empty<IRuleMessage>();

            var result = new List<IRuleMessage>();

            foreach (var textColumn in textColumns)
            {
                var nullableRowCount = GetColumnNullableRowCount(dialect, table, textColumn);
                if (nullableRowCount != tableRowCount)
                    continue;

                var messageText = $"The table '{ table.Name }' has a nullable column '{ textColumn.Name.LocalName }' whose values are always null. Consider removing the column.";
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

        protected IEnumerable<DataType> TextColumnTypes { get; } = new List<DataType> { DataType.String, DataType.Text, DataType.Unicode, DataType.UnicodeText };

        private const string RuleTitle = "Numeric values stored in a text column.";
    }
}
