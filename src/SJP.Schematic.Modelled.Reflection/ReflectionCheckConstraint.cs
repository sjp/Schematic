using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionCheckConstraint : IDatabaseCheckConstraint
    {
        public ReflectionCheckConstraint(IRelationalDatabaseTable table, Identifier name, string definition)
        {
            if (name == null || name.LocalName == null)
                throw new ArgumentNullException(nameof(name));
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Table = table ?? throw new ArgumentNullException(nameof(table));
            Name = name.LocalName;
            Definition = definition;
        }

        public string Definition { get; }

        public Identifier Name { get; }

        public IRelationalDatabaseTable Table { get; }

        public bool IsEnabled { get; } = true;
    }
}
