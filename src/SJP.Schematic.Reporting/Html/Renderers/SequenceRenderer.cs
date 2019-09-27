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
    internal sealed class SequenceRenderer : ITemplateRenderer
    {
        public SequenceRenderer(
            IIdentifierDefaults identifierDefaults,
            IHtmlFormatter formatter,
            IReadOnlyCollection<IDatabaseSequence> sequences,
            DirectoryInfo exportDirectory
        )
        {
            if (sequences == null || sequences.AnyNull())
                throw new ArgumentNullException(nameof(sequences));

            Sequences = sequences;

            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

            if (exportDirectory == null)
                throw new ArgumentNullException(nameof(exportDirectory));

            ExportDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "sequences"));
        }

        private IIdentifierDefaults IdentifierDefaults { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IDatabaseSequence> Sequences { get; }

        private DirectoryInfo ExportDirectory { get; }

        public Task RenderAsync(CancellationToken cancellationToken = default)
        {
            var mapper = new SequenceModelMapper();

            var sequenceTasks = Sequences.Select(async sequence =>
            {
                var viewModel = mapper.Map(sequence);
                var renderedSequence = await Formatter.RenderTemplateAsync(viewModel).ConfigureAwait(false);

                var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                    ? IdentifierDefaults.Database + " Database"
                    : "Database";
                var pageTitle = sequence.Name.ToVisibleName() + " · Sequence · " + databaseName;
                var sequenceContainer = new Container(renderedSequence, pageTitle, "../");
                var renderedPage = await Formatter.RenderTemplateAsync(sequenceContainer).ConfigureAwait(false);

                var outputPath = Path.Combine(ExportDirectory.FullName, sequence.Name.ToSafeKey() + ".html");
                if (!ExportDirectory.Exists)
                    ExportDirectory.Create();

                using var writer = File.CreateText(outputPath);
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            });

            return Task.WhenAll(sequenceTasks);
        }
    }
}
