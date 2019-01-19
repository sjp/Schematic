using System;
using System.Collections.Generic;
using Superpower.Model;
using System.Linq;
using SJP.Schematic.Core.Extensions;
using LanguageExt;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class Check
    {
        public Check(Option<string> constraintName, IEnumerable<Token<SqliteToken>> definition)
        {
            if (definition == null || definition.Empty())
                throw new ArgumentNullException(nameof(definition));

            Name = constraintName;
            Definition = definition.ToList();
        }

        public Option<string> Name { get; }

        public IEnumerable<Token<SqliteToken>> Definition { get; }
    }
}
