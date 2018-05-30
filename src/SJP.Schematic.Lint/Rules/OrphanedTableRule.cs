using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class OrphanedTableRule : Rule
    {
        public OrphanedTableRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

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

            if (table.ParentKeys.Any())
                return Enumerable.Empty<IRuleMessage>();

            if (table.ChildKeys.Any())
                return Enumerable.Empty<IRuleMessage>();

            var message = BuildMessage(table.Name);
            return new[] { message };
        }

        protected virtual IRuleMessage BuildMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageText = $"The table { tableName } is not related to any other table. Consider adding relations or removing the table.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "No relations on a table. The table is orphaned.";
    }
}
