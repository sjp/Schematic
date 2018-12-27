using System;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite
{
    public class SqliteCheckConstraint : IDatabaseCheckConstraint
    {
        public SqliteCheckConstraint(Option<Identifier> checkName, string definition)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Name = checkName.Map(name => new Identifier(name.LocalName));
            Definition = definition;
        }

        public Option<Identifier> Name { get; }

        public string Definition { get; }

        public bool IsEnabled { get; } = true;
    }
}
