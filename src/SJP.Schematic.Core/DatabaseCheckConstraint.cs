using System;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    public class DatabaseCheckConstraint : IDatabaseCheckConstraint
    {
        public DatabaseCheckConstraint(Identifier checkName, string definition, bool isEnabled)
        {
            if (checkName == null)
                throw new ArgumentNullException(nameof(checkName));
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Name = checkName.LocalName;
            Definition = definition;
            IsEnabled = isEnabled;
        }

        public Identifier Name { get; }

        public string Definition { get; }

        public bool IsEnabled { get; }
    }
}
