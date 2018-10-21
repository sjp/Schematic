using System.Data;

namespace SJP.Schematic.Core
{
    public interface IDatabaseRelationalKey
    {
        Identifier ParentTable { get; }

        IDatabaseKey ParentKey { get; }

        Identifier ChildTable { get; }

        IDatabaseKey ChildKey { get; }

        Rule UpdateRule { get; }

        Rule DeleteRule { get; }
    }
}
