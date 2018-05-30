using SJP.Schematic.Core;
using System.Collections.Generic;

namespace SJP.Schematic.Reporting.Dot
{
    public interface IDatabaseDotFormatter
    {
        string RenderDatabase();

        string RenderDatabase(DotRenderOptions options);

        string RenderTables(IEnumerable<IRelationalDatabaseTable> tables);

        string RenderTables(IEnumerable<IRelationalDatabaseTable> tables, DotRenderOptions options);
    }
}
