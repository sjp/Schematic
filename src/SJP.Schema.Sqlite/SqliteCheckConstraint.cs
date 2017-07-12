using System;
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
            Definition = definition;
        }

        public IRelationalDatabaseTable Table { get; }

        public Identifier Name { get; }

        public string Definition { get; }

        public bool IsEnabled { get; } = true;
    }
}
