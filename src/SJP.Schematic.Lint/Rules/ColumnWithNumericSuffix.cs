using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    /// <summary>
    /// A linting rule which reports when columns in a table have a numeric suffix, indicating denormalization.
    /// </summary>
    /// <seealso cref="Rule" />
    /// <seealso cref="ITableRule" />
    public class ColumnWithNumericSuffix : Rule, ITableRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnWithNumericSuffix"/> class.
        /// </summary>
        /// <param name="level">The reporting level.</param>
        public ColumnWithNumericSuffix(RuleLevel level)
            : base(RuleId, RuleTitle, level)
        {
        }

        /// <summary>
        /// Analyses database tables. Reports messages when columns have numeric suffixes.
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
        /// Analyses a database table. Reports messages when columns have numeric suffixes.
        /// </summary>
        /// <param name="table">A database table.</param>
        /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var columnsWithNumericSuffix = table.Columns
                .Select(c => c.Name.LocalName)
                .Where(c => NumericSuffixRegex.IsMatch(c))
                .ToList();
            if (columnsWithNumericSuffix.Empty())
                return Array.Empty<IRuleMessage>();

            return columnsWithNumericSuffix
                .ConvertAll(c => BuildMessage(table.Name, c))
;
        }

        /// <summary>
        /// Builds the message used for reporting.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column with a numeric suffix.</param>
        /// <returns>A formatted linting message.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c> or <paramref name="columnName"/> is <c>null</c>, empty or whitespace.</exception>
        protected virtual IRuleMessage BuildMessage(Identifier tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var messageText = $"The table '{ tableName }' has a column '{ columnName }' with a numeric suffix, indicating denormalization.";
            return new RuleMessage(RuleId, RuleTitle, Level, messageText);
        }

        /// <summary>
        /// The rule identifier.
        /// </summary>
        /// <value>A rule identifier.</value>
        protected static string RuleId { get; } = "SCHEMATIC0003";

        /// <summary>
        /// Gets the rule title.
        /// </summary>
        /// <value>The rule title.</value>
        protected static string RuleTitle { get; } = "Column with a numeric suffix.";

        private static readonly Regex NumericSuffixRegex = new(".*[0-9]$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    }
}
