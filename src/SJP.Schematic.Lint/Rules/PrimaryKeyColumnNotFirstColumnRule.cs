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
