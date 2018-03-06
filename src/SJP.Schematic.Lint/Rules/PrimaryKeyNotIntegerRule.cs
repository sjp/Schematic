using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class PrimaryKeyNotIntegerRule : Rule
    {
        public PrimaryKeyNotIntegerRule(RuleLevel level)
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
                return Enumerable.Empty<IRuleMessage>();

            var pkColumns = primaryKey.Columns.ToList();
            if (pkColumns.Count == 1 && ColumnIsInteger(pkColumns[0]))
                return Enumerable.Empty<IRuleMessage>();

            var messageText = $"The table { table.Name } has a primary key whose column is not an integer.";
            var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);

            return new[] { ruleMessage };
        }

        protected static bool ColumnIsInteger(IDatabaseColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var integerTypes = new[] { DataType.BigInteger, DataType.Integer, DataType.SmallInteger };
            return integerTypes.Contains(column.Type.DataType);
        }

        private const string RuleTitle = "Table contains a non-integer primary key.";
    }
}
