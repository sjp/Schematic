using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite
{
    public class SqliteCheckConstraint : IDatabaseCheckConstraint
    {
        public SqliteCheckConstraint(Identifier checkName, string definition)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));
            if (checkName == null)
                throw new ArgumentNullException(nameof(checkName));

            Name = checkName.LocalName;
            Definition = definition;
        }

        public Identifier Name { get; }

        public string Definition { get; }

        public bool IsEnabled { get; } = true;
    }
}
