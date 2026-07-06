using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.Renderers;

namespace SJP.Schematic.Reporting.Html;

/// <summary>
/// The JSON-based output contract. A data renderer serializes its viewmodel(s) to canonical
/// JSON, writes the <c>.json</c> file(s), and registers the same payload string with the shared
/// bundle so the served (<c>fetch</c>) and disk (<c>bundle.js</c>) sources cannot drift.
/// </summary>
/// <remarks>
/// <paramref name="data"/> and <paramref name="context"/> supply everything renderers need to know
/// per call: <see cref="ReportData"/> is what to render, <see cref="Renderers.RenderContext"/> is
/// where/how to write it. Constructors are reserved for genuine collaborators — e.g. the
/// dialect-specific <c>IRelationalDatabaseLinter</c> used by <c>LintRenderer</c> — that are not
/// part of the report's data and would not vary between calls.
/// </remarks>
internal interface IDataRenderer
{
    Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default);
}
