using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public interface IDatabaseViewComments
    {
        Identifier ViewName { get; }

        Option<string> Comment { get; }

        IReadOnlyDictionary<Identifier, Option<string>> ColumnComments { get; }
    }
}
