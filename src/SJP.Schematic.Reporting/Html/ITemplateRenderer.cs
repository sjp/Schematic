using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Reporting.Html;

public interface ITemplateRenderer
{
    Task RenderAsync(CancellationToken cancellationToken = default);
}
