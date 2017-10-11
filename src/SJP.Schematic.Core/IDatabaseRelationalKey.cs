using System.Data;

namespace SJP.Schematic.Core
{
    public interface IDatabaseRelationalKey
    {
        IDatabaseKey ParentKey { get; }

        IDatabaseKey ChildKey { get; }

        Rule UpdateRule { get; }

        Rule DeleteRule { get; }
    }
}
