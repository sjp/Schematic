using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schema.Core;

namespace SJP.Schema.SQLite
{
    public class SQLiteCheckConstraint : IDatabaseCheckConstraint
    {
        public SQLiteCheckConstraint(IRelationalDatabaseTable table, Identifier checkName, string definition)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Table = table;
            Name = checkName;
            Expression = null; // TODO: // new ModelledSqlExpression();
        }

        public IEnumerable<IDatabaseColumn> DependentColumns { get; } = Enumerable.Empty<IDatabaseColumn>();

        public ISqlExpression Expression { get; }

        public Identifier Name { get; }

        public IRelationalDatabaseTable Table { get; }
    }

}
