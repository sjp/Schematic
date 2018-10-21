using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlCheckConstraint : IDatabaseCheckConstraint
    {
        public PostgreSqlCheckConstraint(Identifier checkName, string definition)
        {
            if (checkName == null)
                throw new ArgumentNullException(nameof(checkName));
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Name = checkName.LocalName;
            Definition = definition;
        }

        public Identifier Name { get; }

        public string Definition { get; }

        public bool IsEnabled { get; } = true;
    }
}
