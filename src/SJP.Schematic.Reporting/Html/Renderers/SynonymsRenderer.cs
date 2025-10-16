using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class SynonymsRenderer : ITemplateRenderer
{
    public SynonymsRenderer(
        IIdentifierDefaults identifierDefaults,
        IHtmlFormatter formatter,
        IEnumerable<IDatabaseSynonym> synonyms,
        SynonymTargets synonymTargets,
        DirectoryInfo exportDirectory)
    {
        Synonyms = synonyms ?? throw new ArgumentNullException(nameof(synonyms));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        SynonymTargets = synonymTargets ?? throw new ArgumentNullException(nameof(synonymTargets));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IIdentifierDefaults IdentifierDefaults { get; }

    private IHtmlFormatter Formatter { get; }

    private IEnumerable<IDatabaseSynonym> Synonyms { get; }

    private SynonymTargets SynonymTargets { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new MainModelMapper();

        var synonymViewModels = Synonyms.Select(s => mapper.Map(s, SynonymTargets)).ToList();
        var synonymsVm = new Synonyms(synonymViewModels);

        var renderedMain = await Formatter.RenderTemplateAsync(synonymsVm, cancellationToken);

        var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
            ? IdentifierDefaults.Database + " Database"
            : "Database";
        var pageTitle = "Synonyms · " + databaseName;
        var mainContainer = new Container(renderedMain, pageTitle, string.Empty);
        var renderedPage = await Formatter.RenderTemplateAsync(mainContainer, cancellationToken);

        if (!ExportDirectory.Exists)
            ExportDirectory.Create();
        var outputPath = Path.Combine(ExportDirectory.FullName, "synonyms.html");

        await using var writer = File.CreateText(outputPath);
        await writer.WriteAsync(renderedPage.AsMemory(), cancellationToken);
        await writer.FlushAsync(cancellationToken);
    }
}