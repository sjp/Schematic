using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules
{
    /// <summary>
    /// A linting rule which reports when foreign key relationships have mismatching column types for their parent and child key columns.
    /// </summary>
    /// <seealso cref="Rule"/>
    /// <seealso cref="ITableRule"/>
    public class ForeignKeyColumnTypeMismatchRule : Rule, ITableRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForeignKeyColumnTypeMismatchRule"/> class.
        /// </summary>
        /// <param name="level">The reporting level.</param>
        public ForeignKeyColumnTypeMismatchRule(RuleLevel level)
            : base(RuleId, RuleTitle, level)
        {
        }

        /// <summary>
        /// Analyses database tables. Reports messages when foreign key relationships have mismatching column types for their parent and child key columns.
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
        /// Analyses a database table. Reports messages when a table has foreign key relationships with mismatching column types for their parent and child key columns.
        /// </summary>
        /// <param name="table">A database table.</param>
        /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var result = new List<IRuleMessage>();

            var foreignKeys = table.ParentKeys;
            foreach (var foreignKey in foreignKeys)
            {
                var childColumns = foreignKey.ChildKey.Columns;
                var parentColumns = foreignKey.ParentKey.Columns;

                var childColumnsInfo = childColumns.Select(c => c.Type.Definition).ToList();
                var parentColumnsInfo = parentColumns.Select(c => c.Type.Definition).ToList();

                var columnsEqual = childColumnsInfo.SequenceEqual(parentColumnsInfo);
                if (columnsEqual)
                    continue;

                var ruleMessage = BuildMessage(foreignKey.ChildKey.Name, foreignKey.ChildTable, foreignKey.ParentTable);
                result.Add(ruleMessage);
            }

            return result;
        }

        /// <summary>
        /// Builds the message used for reporting.
        /// </summary>
        /// <param name="foreignKeyName">The name of the foreign key constraint, if available.</param>
        /// <param name="childTableName">The name of the child table.</param>
        /// <param name="parentTableName">The name of the parent table.</param>
        /// <returns>A formatted linting message.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="childTableName"/> or <paramref name="parentTableName"/> is <c>null</c>.</exception>
        protected virtual IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier childTableName, Identifier parentTableName)
        {
            if (childTableName == null)
                throw new ArgumentNullException(nameof(childTableName));
            if (parentTableName == null)
                throw new ArgumentNullException(nameof(parentTableName));

            var builder = StringBuilderCache.Acquire();
            builder.Append("A foreign key");
            foreignKeyName.IfSome(name =>
            {
                builder.Append(" '")
                    .Append(name.LocalName)
                    .Append('\'');
            });

            builder.Append(" from ")
                .Append(childTableName)
                .Append(" to ")
                .Append(parentTableName)
                .Append(" contains mismatching column types. These should be the same in order to ensure that foreign keys can always hold the same information as the target key.");

            var messageText = builder.GetStringAndRelease();
            return new RuleMessage(RuleId, RuleTitle, Level, messageText);
        }

        /// <summary>
        /// The rule identifier.
        /// </summary>
        /// <value>A rule identifier.</value>
        protected static string RuleId { get; } = "SCHEMATIC0005";

        /// <summary>
        /// Gets the rule title.
        /// </summary>
        /// <value>The rule title.</value>
        protected static string RuleTitle { get; } = "Foreign key relationships contain mismatching types.";
    }
}
