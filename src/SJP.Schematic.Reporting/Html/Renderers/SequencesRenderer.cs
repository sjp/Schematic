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

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class SequencesRenderer : ITemplateRenderer
    {
        public SequencesRenderer(
            IIdentifierDefaults identifierDefaults,
            IHtmlFormatter formatter,
            IReadOnlyCollection<IDatabaseSequence> sequences,
            DirectoryInfo exportDirectory)
        {
            if (sequences == null || sequences.AnyNull())
                throw new ArgumentNullException(nameof(sequences));

            Sequences = sequences;

            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IIdentifierDefaults IdentifierDefaults { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IDatabaseSequence> Sequences { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default)
        {
            var mapper = new MainModelMapper();

            var sequenceViewModels = Sequences.Select(mapper.Map).ToList();
            var sequencesVm = new Sequences(sequenceViewModels);

            var renderedMain = await Formatter.RenderTemplateAsync(sequencesVm).ConfigureAwait(false);

            var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? IdentifierDefaults.Database + " Database"
                : "Database";
            var pageTitle = "Sequences · " + databaseName;
            var mainContainer = new Container(renderedMain, pageTitle, string.Empty);
            var renderedPage = await Formatter.RenderTemplateAsync(mainContainer).ConfigureAwait(false);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "sequences.html");

            using var writer = File.CreateText(outputPath);
            await writer.WriteAsync(renderedPage).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
        }
    }
}
