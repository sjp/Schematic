using System;
using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.SqlServer
{
    public class SqlServerCheckConstraint : IDatabaseCheckConstraint
    {
        public SqlServerCheckConstraint(IRelationalDatabaseTable table, Identifier checkName, string definition, IEnumerable<IDatabaseColumn> columns)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));

            Table = table ?? throw new ArgumentNullException(nameof(table));
            Name = checkName ?? throw new ArgumentNullException(nameof(checkName));
            Expression = ParseExpression(definition);
            DependentColumns = columns;
        }

        public IEnumerable<IDatabaseColumn> DependentColumns { get; }

        public ISqlExpression Expression { get; }

        public Identifier Name { get; }

        public IRelationalDatabaseTable Table { get; }

        // TODO: implement so that check constraint expressions are easy!
        private static ISqlExpression ParseExpression(string definition)
        {
            // TODO use sprache and/or superpower


            return null;
        }
    }
}
