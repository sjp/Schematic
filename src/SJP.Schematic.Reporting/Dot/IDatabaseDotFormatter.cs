using SJP.Schematic.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Reporting.Dot
{
    public interface IDatabaseDotFormatter
    {
        Task<string> RenderTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken);

        Task<string> RenderTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, DotRenderOptions options, CancellationToken cancellationToken);
    }
}
