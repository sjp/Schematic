using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules
{
    /// <summary>
    /// A linting rule which reports when foreign key relationships are implied, but not enforced by a foreign key constraint.
    /// </summary>
    /// <seealso cref="Rule"/>
    /// <seealso cref="ITableRule"/>
    public class ForeignKeyMissingRule : Rule, ITableRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForeignKeyMissingRule"/> class.
        /// </summary>
        /// <param name="level">The reporting level.</param>
        public ForeignKeyMissingRule(RuleLevel level)
            : base(RuleId, RuleTitle, level)
        {
        }

        /// <summary>
        /// Analyses database tables. Reports messages when a foreign key relationship is implied, but missing a foreign key constraint to enforce it.
        /// </summary>
        /// <param name="tables">A set of database tables.</param>
        /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
        /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <c>null</c>.</exception>
        public IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            var tableNames = tables.Select(t => t.Name).ToList();
            return tables.SelectMany(t => AnalyseTable(t, tableNames)).ToAsyncEnumerable();
        }

        /// <summary>
        /// Analyses a database table. Reports messages when columns have numeric suffixes.
        /// </summary>
        /// <param name="table">A database table.</param>
        /// <param name="tableNames">Other table names in the database.</param>
        /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> or <paramref name="tableNames"/> is <c>null</c>.</exception>
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
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var columnNames = table.Columns.Select(c => c.Name.LocalName);

            foreach (var columnName in columnNames)
            {
                var impliedTable = GetImpliedTableName(columnName);
                var targetTableName = tableNames.FirstOrDefault(t => string.Equals(impliedTable, t.LocalName, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(impliedTable, table.Name.LocalName, StringComparison.OrdinalIgnoreCase));
                if (targetTableName == null)
                    continue;

                // now check whether the column name is already part of an FK
                if (foreignKeyColumnNames.Contains(columnName))
                    continue;

                var message = BuildMessage(columnName, table.Name, targetTableName);
                result.Add(message);
            }

            return result;
        }

        /// <summary>
        /// Gets the name of the implied table.
        /// </summary>
        /// <param name="columnName">The name of the column that can imply a table name.</param>
        /// <returns>The implied table name if found, otherwise the value of <paramref name="columnName"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="columnName"/> is <c>null</c>, empty or whitespace.</exception>
        protected static string GetImpliedTableName(string columnName)
        {
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            const string snakeCaseSuffix = "_id";
            if (columnName.EndsWith(snakeCaseSuffix, StringComparison.OrdinalIgnoreCase))
                return columnName[..^snakeCaseSuffix.Length];

            const string camelCaseSuffix = "Id";
            if (columnName.EndsWith(camelCaseSuffix, StringComparison.Ordinal))
                return columnName[..^camelCaseSuffix.Length];

            return columnName;
        }

        /// <summary>
        /// Builds the message used for reporting.
        /// </summary>
        /// <param name="columnName">The name of the column that implies a foreign key relationship.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="targetTableName">The implied target table.</param>
        /// <returns>A formatted linting message.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="targetTableName"/> is <c>null</c>. Also thrown when <paramref name="columnName"/> is <c>null</c>, empty or whitespace.</exception>
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
            return new RuleMessage(RuleId, RuleTitle, Level, messageText);
        }

        /// <summary>
        /// The rule identifier.
        /// </summary>
        /// <value>A rule identifier.</value>
        protected static string RuleId { get; } = "SCHEMATIC0008";

        /// <summary>
        /// Gets the rule title.
        /// </summary>
        /// <value>The rule title.</value>
        protected static string RuleTitle { get; } = "Column name implies a relationship missing a foreign key constraint.";
    }
}
