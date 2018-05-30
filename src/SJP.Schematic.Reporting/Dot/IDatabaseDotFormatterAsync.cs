using SJP.Schematic.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schematic.Reporting.Dot
{
    public interface IDatabaseDotFormatterAsync
    {
        Task<string> RenderDatabaseAsync();

        Task<string> RenderDatabaseAsync(DotRenderOptions options);

        Task<string> RenderTablesAsync(IEnumerable<IRelationalDatabaseTable> tables);

        Task<string> RenderTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, DotRenderOptions options);
    }
}
