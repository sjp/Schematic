using System.Collections.Generic;

namespace SJP.Schematic.Migrations
{
    public interface ISqlCommand
    {
        string Query { get; }

        IReadOnlyDictionary<string, object> Parameters { get; }
    }
}