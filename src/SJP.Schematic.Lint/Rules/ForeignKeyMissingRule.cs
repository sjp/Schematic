using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules
{
    public class ForeignKeyMissingRule : Rule, ITableRule
    {
        public ForeignKeyMissingRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public IEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            var tableNames = tables.Select(t => t.Name).ToList();
            return tables.SelectMany(t => AnalyseTable(t, tableNames)).ToList();
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            var messages = AnalyseTables(tables);
            return Task.FromResult(messages);
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table, IEnumerable<Identifier> tableNames)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (tableNames == null)
                throw new ArgumentNullException(nameof(tableNames));

            var result = new List<IRuleMessage>();

            var foreignKeyColumnNames = table.ParentKeys
                .Select(fk => fk.ChildKey)
                .SelectMany(fk => fk.Columns)
                .Select(c => c.Name.LocalName)
                .Distinct()
                .ToList();

            foreach (var column in table.Columns)
            {
                var impliedTable = GetImpliedTableName(column.Name.LocalName);
                var targetTableName = tableNames.FirstOrDefault(t => string.Equals(impliedTable, t.LocalName, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(impliedTable, table.Name.LocalName, StringComparison.OrdinalIgnoreCase));
                if (targetTableName == null)
                    continue;

                // now check whether the column name is already part of an FK
                if (foreignKeyColumnNames.Contains(column.Name.LocalName))
                    continue;

                var message = BuildMessage(column.Name.LocalName, table.Name, targetTableName);
                result.Add(message);
            }

            return result;
        }

        // intended to parse out the table name from the column name
        protected static string GetImpliedTableName(string columnName)
        {
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            const string snakeCaseSuffix = "_id";
            if (columnName.EndsWith(snakeCaseSuffix, StringComparison.OrdinalIgnoreCase))
                return columnName.Substring(0, columnName.Length - snakeCaseSuffix.Length);

            const string camelCaseSuffix = "Id";
            if (columnName.EndsWith(camelCaseSuffix, StringComparison.Ordinal))
                return columnName.Substring(0, columnName.Length - camelCaseSuffix.Length);

            return columnName;
        }

        protected virtual IRuleMessage BuildMessage(string columnName, Identifier tableName, Identifier targetTableName)
        {
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (targetTableName == null)
                throw new ArgumentNullException(nameof(targetTableName));

            var builder = StringBuilderCache.Acquire();

            builder.Append("The table ")
                .Append(tableName)
                .Append(" has a column ")
                .Append(columnName)
                .Append(" implying a relationship to ")
                .Append(targetTableName)
                .Append(" which is missing a foreign key constraint.");

            var messageText = builder.GetStringAndRelease();
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Column name implies a relationship missing a foreign key constraint.";
    }
}
