using SJP.Schematic.Core;
using System.Collections.Generic;

namespace SJP.Schematic.Reporting.Dot
{
    public interface IDatabaseDotFormatter
    {
        string RenderTables(IEnumerable<IRelationalDatabaseTable> tables, IReadOnlyDictionary<Identifier, ulong> rowCounts);

        string RenderTables(IEnumerable<IRelationalDatabaseTable> tables, IReadOnlyDictionary<Identifier, ulong> rowCounts, DotRenderOptions options);
    }
}
