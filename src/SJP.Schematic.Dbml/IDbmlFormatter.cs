using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Dbml
{
    public interface IDbmlFormatter
    {
        string RenderTables(IEnumerable<IRelationalDatabaseTable> tables);
    }
}