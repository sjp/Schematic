using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class PrimaryKeyColumnNotFirstColumnRule : Rule
    {
        public PrimaryKeyColumnNotFirstColumnRule(RuleLevel level)
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

            var primaryKey = table.PrimaryKey;
            if (primaryKey == null)
                return Array.Empty<IRuleMessage>();

            var pkColumns = primaryKey.Columns.ToList();
            if (pkColumns.Count != 1)
                return Array.Empty<IRuleMessage>();

            var tableColumns = table.Columns;
            if (tableColumns.Empty())
                return Array.Empty<IRuleMessage>();

            var pkColumnName = pkColumns[0].Name;
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
