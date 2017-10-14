using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled
{
    public class ModelledCheckConstraint : IDatabaseCheckConstraint
    {
        public ModelledCheckConstraint(IRelationalDatabaseTable table, Identifier checkName, string definition, bool isEnabled)
        {
            if (checkName == null || checkName.LocalName == null)
                throw new ArgumentNullException(nameof(checkName));
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Table = table ?? throw new ArgumentNullException(nameof(table));
            Name = checkName.LocalName;
            Definition = definition;
            IsEnabled = isEnabled;
        }

        public IRelationalDatabaseTable Table { get; }

        public Identifier Name { get; }

        public string Definition { get; }

        public bool IsEnabled { get; }
    }
}
