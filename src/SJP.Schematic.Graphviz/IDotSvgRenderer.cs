using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Graphviz
{
    public interface IDotSvgRenderer
    {
        string RenderToSvg(string dot);

        Task<string> RenderToSvgAsync(string dot, CancellationToken cancellationToken = default);
    }
}
