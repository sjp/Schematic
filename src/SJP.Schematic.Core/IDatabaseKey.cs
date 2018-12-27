using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IDatabaseKey : IDatabaseOptional
    {
        Option<Identifier> Name { get; }

        IReadOnlyCollection<IDatabaseColumn> Columns { get; }

        DatabaseKeyType KeyType { get; }
    }
}
