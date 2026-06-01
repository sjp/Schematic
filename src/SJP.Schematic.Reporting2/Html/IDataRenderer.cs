using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Reporting.Html;

/// <summary>
/// Replaces <see cref="ITemplateRenderer"/> for the JSON-based output pipeline. A data
/// renderer serializes its viewmodel(s) to canonical JSON, writes the <c>.json</c> file(s),
/// and registers the same payload string with the shared bundle so the served (<c>fetch</c>)
/// and disk (<c>bundle.js</c>) sources cannot drift.
/// </summary>
/// <remarks>
/// The signature mirrors <see cref="ITemplateRenderer.RenderAsync"/> so that
/// <c>ReportGenerator</c> orchestration is unchanged. Concrete renderers receive a
/// <see cref="Serialization.JsonDataWriter"/> and the shared
/// <see cref="Serialization.BundleBuilder"/> via their constructors.
/// </remarks>
public interface IDataRenderer
{
    Task RenderAsync(CancellationToken cancellationToken = default);
}
