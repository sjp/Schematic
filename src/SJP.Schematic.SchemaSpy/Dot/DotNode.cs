using SJP.Schematic.Core;
using System;

namespace SJP.Schematic.SchemaSpy.Dot
{
    public interface IDotNode
    {
        IRelationalDatabaseTable Table { get; }
    }

    public class DotNode : IDotNode
    {
        public DotNode(IRelationalDatabaseTable table)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }
}
