using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class ColumnWithNullDefaultValueRule : Rule, ITableRule
    {
        public ColumnWithNullDefaultValueRule(RuleLevel level)
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

            var nullableColumns = table.Columns.Where(c => c.IsNullable).ToList();
            if (nullableColumns.Empty())
                return Array.Empty<IRuleMessage>();

            var result = new List<IRuleMessage>();

            foreach (var nullableColumn in nullableColumns)
            {
                nullableColumn.DefaultValue
                    .Where(IsNullDefaultValue)
                    .IfSome(_ =>
                {
                    var ruleMessage = BuildMessage(table.Name, nullableColumn.Name.LocalName);
                    result.Add(ruleMessage);
                });
            }

            return result;
        }

        protected static bool IsNullDefaultValue(string defaultValue)
        {
            return !defaultValue.IsNullOrWhiteSpace()
                && NullValues.Contains(defaultValue);
        }

        protected virtual IRuleMessage BuildMessage(Identifier tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var messageText = $"The table '{ tableName }' has a column '{ columnName }' whose default value is null. Consider removing the default value on the column.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Null default values assigned to column.";
        private static readonly IEnumerable<string> NullValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "null", "(null)" };
    }
}
