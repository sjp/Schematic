using System;
using LanguageExt;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    public class DatabaseCheckConstraint : IDatabaseCheckConstraint
    {
        public DatabaseCheckConstraint(Option<Identifier> checkName, string definition, bool isEnabled)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Name = checkName.Map(name => Identifier.CreateQualifiedIdentifier(name.LocalName));
            Definition = definition;
            IsEnabled = isEnabled;
        }

        public Option<Identifier> Name { get; }

        public string Definition { get; }

        public bool IsEnabled { get; }
    }
}
