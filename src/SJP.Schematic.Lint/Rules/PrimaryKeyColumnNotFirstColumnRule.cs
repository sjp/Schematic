using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class PrimaryKeyColumnNotFirstColumnRule : Rule, ITableRule
    {
        public PrimaryKeyColumnNotFirstColumnRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public IEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return tables.SelectMany(AnalyseTable).ToList();
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            var messages = AnalyseTables(tables);
            return Task.FromResult(messages);
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return table.PrimaryKey
                .Where(pk => pk.Columns.Count == 1)
                .Match(pk => AnalyseTable(table, pk), Array.Empty<IRuleMessage>);
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table, IDatabaseKey primaryKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (primaryKey == null)
                throw new ArgumentNullException(nameof(primaryKey));

            var tableColumns = table.Columns;
            if (tableColumns.Empty())
                return Array.Empty<IRuleMessage>();

            var pkColumnName = primaryKey.Columns.Single().Name;
            var firstColumnName = table.Columns[0].Name;
            if (pkColumnName == firstColumnName)
                return Array.Empty<IRuleMessage>();

            var message = BuildMessage(table.Name);
            return new[] { message };
        }

        protected virtual IRuleMessage BuildMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageText = $"The table { tableName } has a primary key whose column is not the first column in the table.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Table primary key whose only column is not the first column in the table.";
    }
}
