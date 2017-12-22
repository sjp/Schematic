using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlCheckConstraint : IDatabaseCheckConstraint
    {
        public PostgreSqlCheckConstraint(IRelationalDatabaseTable table, Identifier checkName, string definition)
        {
            if (checkName == null || checkName.LocalName == null)
                throw new ArgumentNullException(nameof(checkName));
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Table = table ?? throw new ArgumentNullException(nameof(table));
            Name = checkName.LocalName;
            Definition = definition;
        }

        public IRelationalDatabaseTable Table { get; }

        public Identifier Name { get; }

        public string Definition { get; }

        public bool IsEnabled { get; } = true;
    }
}
