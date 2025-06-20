using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Reporting.Html;

public interface IHtmlFormatter
{
    Task<string> RenderTemplateAsync<T>(T templateParameter, CancellationToken cancellationToken = default) where T : ITemplateParameter;
}