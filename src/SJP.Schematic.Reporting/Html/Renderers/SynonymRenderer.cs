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

internal sealed class SynonymRenderer : ITemplateRenderer
{
    public SynonymRenderer(
        IIdentifierDefaults identifierDefaults,
        IHtmlFormatter formatter,
        IEnumerable<IDatabaseSynonym> synonyms,
        SynonymTargets synonymTargets,
        DirectoryInfo exportDirectory
    )
    {
        if (exportDirectory == null)
            throw new ArgumentNullException(nameof(exportDirectory));

        Synonyms = synonyms ?? throw new ArgumentNullException(nameof(synonyms));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        SynonymTargets = synonymTargets ?? throw new ArgumentNullException(nameof(synonymTargets));
        ExportDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "synonyms"));
    }

    private IIdentifierDefaults IdentifierDefaults { get; }

    private IHtmlFormatter Formatter { get; }

    private IEnumerable<IDatabaseSynonym> Synonyms { get; }

    private SynonymTargets SynonymTargets { get; }

    private DirectoryInfo ExportDirectory { get; }

    public Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new SynonymModelMapper();

        var synonymTasks = Synonyms.Select(async synonym =>
        {
            var viewModel = mapper.Map(synonym, SynonymTargets);
            var renderedSynonym = await Formatter.RenderTemplateAsync(viewModel, cancellationToken).ConfigureAwait(false);

            var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? IdentifierDefaults.Database + " Database"
                : "Database";
            var pageTitle = synonym.Name.ToVisibleName() + " · Synonym · " + databaseName;
            var synonymContainer = new Container(renderedSynonym, pageTitle, "../");
            var renderedPage = await Formatter.RenderTemplateAsync(synonymContainer, cancellationToken).ConfigureAwait(false);

            var outputPath = Path.Combine(ExportDirectory.FullName, synonym.Name.ToSafeKey() + ".html");
            if (!ExportDirectory.Exists)
                ExportDirectory.Create();

            using var writer = File.CreateText(outputPath);
            await writer.WriteAsync(renderedPage.AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
        });

        return Task.WhenAll(synonymTasks);
    }
}