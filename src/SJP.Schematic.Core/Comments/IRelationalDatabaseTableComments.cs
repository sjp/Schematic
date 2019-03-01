using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public interface IRelationalDatabaseTableComments
    {
        Identifier TableName { get; }

        Option<string> Comment { get; }

        Option<string> PrimaryKeyComment { get; }

        IReadOnlyDictionary<Identifier, Option<string>> ColumnComments { get; }

        IReadOnlyDictionary<Identifier, Option<string>> CheckComments { get; }

        IReadOnlyDictionary<Identifier, Option<string>> UniqueKeyComments { get; }

        IReadOnlyDictionary<Identifier, Option<string>> ForeignKeyComments { get; }

        IReadOnlyDictionary<Identifier, Option<string>> IndexComments { get; }

        IReadOnlyDictionary<Identifier, Option<string>> TriggerComments { get; }
    }
}
