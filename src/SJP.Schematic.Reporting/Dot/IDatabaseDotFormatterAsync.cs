using SJP.Schematic.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Reporting.Dot
{
    public interface IDatabaseDotFormatterAsync
    {
        Task<string> RenderDatabaseAsync(CancellationToken cancellationToken);

        Task<string> RenderDatabaseAsync(DotRenderOptions options, CancellationToken cancellationToken);

        Task<string> RenderTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken);

        Task<string> RenderTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, DotRenderOptions options, CancellationToken cancellationToken);
    }
}
