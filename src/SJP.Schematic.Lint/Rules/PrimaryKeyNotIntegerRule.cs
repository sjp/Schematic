using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class PrimaryKeyNotIntegerRule : Rule, ITableRule
    {
        public PrimaryKeyNotIntegerRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return tables.SelectMany(AnalyseTable).ToAsyncEnumerable();
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return table.PrimaryKey
                .Where(pk => pk.Columns.Count != 1 || !ColumnIsInteger(pk.Columns.First()))
                .Match(_ => new[] { BuildMessage(table.Name) }, Array.Empty<IRuleMessage>);
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
