using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    /// <summary>
    /// A linting rule which reports when the columns in a primary key are not the first columns in a table.
    /// </summary>
    /// <seealso cref="Rule"/>
    /// <seealso cref="ITableRule"/>
    public class PrimaryKeyColumnNotFirstColumnRule : Rule, ITableRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryKeyColumnNotFirstColumnRule"/> class.
        /// </summary>
        /// <param name="level">The reporting level.</param>
        public PrimaryKeyColumnNotFirstColumnRule(RuleLevel level)
            : base(RuleId, RuleTitle, level)
        {
        }

        /// <summary>
        /// Analyses database tables. Reports messages when the primary key columns are not the first columns in a table.
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
        /// Analyses a database table. Reports messages when the primary key columns are not the first columns in the table.
        /// </summary>
        /// <param name="table">A database table.</param>
        /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return table.PrimaryKey
                .Where(pk => pk.Columns.Count == 1)
                .Match(pk => AnalyseTable(table, pk), Array.Empty<IRuleMessage>);
        }

        /// <summary>
        /// Analyses a database table and its primary key. Reports messages when the primary key columns are not the first columns in the table.
        /// </summary>
        /// <param name="table">A database table.</param>
        /// <param name="primaryKey">The primary key for the database table.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> or <paramref name="primaryKey"/> is <c>null</c>.</exception>
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

        /// <summary>
        /// Builds the message used for reporting.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>A formatted linting message.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected virtual IRuleMessage BuildMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageText = $"The table { tableName } has a primary key whose column is not the first column in the table.";
            return new RuleMessage(RuleId, RuleTitle, Level, messageText);
        }

        /// <summary>
        /// The rule identifier.
        /// </summary>
        /// <value>A rule identifier.</value>
        protected static string RuleId { get; } = "SCHEMATIC0017";

        /// <summary>
        /// Gets the rule title.
        /// </summary>
        /// <value>The rule title.</value>
        protected static string RuleTitle { get; } = "Table primary key whose only column is not the first column in the table.";
    }
}
