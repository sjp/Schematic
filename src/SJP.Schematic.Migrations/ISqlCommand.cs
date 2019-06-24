using System.Collections.Generic;

namespace SJP.Schematic.Migrations
{
    public interface ISqlCommand
    {
        string Sql { get; }

        IReadOnlyDictionary<string, object> Parameters { get; }
    }
}