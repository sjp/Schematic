using System;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql
{
    public class MySqlCheckConstraint : IDatabaseCheckConstraint
    {
        public MySqlCheckConstraint(Identifier checkName, string definition, bool isEnabled)
        {
            if (checkName == null)
                throw new ArgumentNullException(nameof(checkName));
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Name = Option<Identifier>.Some(checkName.LocalName);
            Definition = definition;
            IsEnabled = isEnabled;
        }

        public Option<Identifier> Name { get; }

        public string Definition { get; }

        public bool IsEnabled { get; }
    }
}
