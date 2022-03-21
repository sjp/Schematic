using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Graphviz;

/// <summary>
/// Defines a renderer of DOT files that renders as an SVG image.
/// </summary>
public interface IDotSvgRenderer
{
    /// <summary>
    /// Renders a DOT file as an SVG image synchronously.
    /// </summary>
    /// <param name="dot">A dot graph in string form.</param>
    /// <returns>A rendered SVG image as a string.</returns>
    string RenderToSvg(string dot);

    /// <summary>
    /// Renders a DOT file as an SVG image asynchronously.
    /// </summary>
    /// <param name="dot">A dot graph in string form.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A rendered SVG image as a string.</returns>
    Task<string> RenderToSvgAsync(string dot, CancellationToken cancellationToken = default);
}