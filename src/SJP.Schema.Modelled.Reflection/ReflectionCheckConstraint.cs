using System;
using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionCheckConstraint : IDatabaseCheckConstraint
    {
        public ReflectionCheckConstraint(IRelationalDatabaseTable table, Identifier name, string definition)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Table = table ?? throw new ArgumentNullException(nameof(table));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Definition = definition;
        }

        public string Definition { get; }

        public Identifier Name { get; }

        public IRelationalDatabaseTable Table { get; }

        // this should always be true
        // is there a situation where would not want it to be true?
        public bool IsEnabled { get; } = true;
    }
}
