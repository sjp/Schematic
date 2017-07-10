using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schema.Core;

namespace SJP.Schema.Sqlite
{
    public class SqliteCheckConstraint : IDatabaseCheckConstraint
    {
        public SqliteCheckConstraint(IRelationalDatabaseTable table, Identifier checkName, string definition)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Table = table ?? throw new ArgumentNullException(nameof(table));
            Name = checkName ?? throw new ArgumentNullException(nameof(checkName));
            Expression = null; // TODO: // new ModelledSqlExpression();
        }

        public IEnumerable<IDatabaseColumn> DependentColumns { get; } = Enumerable.Empty<IDatabaseColumn>();

        public ISqlExpression Expression { get; }

        public Identifier Name { get; }

        public IRelationalDatabaseTable Table { get; }

        public bool IsEnabled { get; } = true;
    }
}
