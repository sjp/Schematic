using System;
using System.IO;
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting.Html.Renderers;

/// <summary>
/// The output collaborators shared by every renderer in a report run: where JSON is serialized to,
/// where the served/bundled payloads are registered, and the root directory the report is exported
/// to. Unlike <see cref="ReportData"/>, these do not describe what to render.
/// </summary>
internal sealed class RenderContext
{
    public RenderContext(
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory
    )
    {
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    public JsonDataWriter JsonWriter { get; }

    public BundleBuilder Bundle { get; }

    public DirectoryInfo ExportDirectory { get; }
}
