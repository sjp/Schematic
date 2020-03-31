using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    /// <summary>
    /// A linting rule which reports when a table does not contain multiple columns.
    /// </summary>
    /// <seealso cref="Rule"/>
    /// <seealso cref="ITableRule"/>
    public class OnlyOneColumnPresentRule : Rule, ITableRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnlyOneColumnPresentRule"/> class.
        /// </summary>
        /// <param name="level">The reporting level.</param>
        public OnlyOneColumnPresentRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        /// <summary>
        /// Analyses database tables. Reports messages when a table does not contain multiple columns.
        /// </summary>
        /// <param name="tables">A set of database tables.</param>
        /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
        /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <c>null</c>.</exception>
        public IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return tables.SelectMany(AnalyseTable).ToAsyncEnumerable();
        }

        /// <summary>
        /// Analyses a database table. Reports messages when the table does not have multiple columns.
        /// </summary>
        /// <param name="table">A database table.</param>
        /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var columnCount = table.Columns.Count;
            if (columnCount > 1)
                return Array.Empty<IRuleMessage>();

            var message = BuildMessage(table.Name, columnCount);
            return new[] { message };
        }

        /// <summary>
        /// Builds the message used for reporting.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnCount">The number of columns in the table.</param>
        /// <returns>A formatted linting message.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected virtual IRuleMessage BuildMessage(Identifier tableName, int columnCount)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageText = columnCount == 0
                ? $"The table { tableName } has too few columns. It has no columns, consider adding more."
                : $"The table { tableName } has too few columns. It has one column, consider adding more.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        /// <summary>
        /// Gets the rule title.
        /// </summary>
        /// <value>The rule title.</value>
        protected static string RuleTitle { get; } = "Only one column present on table.";
    }
}
