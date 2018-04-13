using SJP.Schematic.Core;
using System;

namespace SJP.Schematic.SchemaSpy.Dot
{
    public interface IDotConnector
    {
        IDatabaseRelationalKey Key { get; }
    }

    public class DotConnector : IDotConnector
    {
        public DotConnector(IDatabaseRelationalKey relationalKey)
        {
            Key = relationalKey ?? throw new ArgumentNullException(nameof(relationalKey));
        }

        public IDatabaseRelationalKey Key { get; }
    }
}
