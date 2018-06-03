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
                return Array.Empty<IRuleMessage>();

            var pkColumns = primaryKey.Columns.ToList();
            if (pkColumns.Count == 1 && ColumnIsInteger(pkColumns[0]))
                return Array.Empty<IRuleMessage>();

            var message = BuildMessage(table.Name);
            return new[] { message };
        }

        protected static bool ColumnIsInteger(IDatabaseColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var integerTypes = new[] { DataType.BigInteger, DataType.Integer, DataType.SmallInteger };
            return integerTypes.Contains(column.Type.DataType);
        }

        protected virtual IRuleMessage BuildMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageText = $"The table { tableName } has a primary key which is not a single-column whose type is an integer.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Table contains a non-integer primary key.";
    }
}
