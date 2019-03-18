using System;
using System.Collections.Generic;
using System.Data;
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
            IRelationalDatabase database,
            IHtmlFormatter formatter,
            IReadOnlyCollection<IDatabaseSequence> sequences,
            DirectoryInfo exportDirectory
        )
        {
            if (sequences == null || sequences.AnyNull())
                throw new ArgumentNullException(nameof(sequences));

            Sequences = sequences;

            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

            if (exportDirectory == null)
                throw new ArgumentNullException(nameof(exportDirectory));

            ExportDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "sequences"));
        }

        private IRelationalDatabase Database { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IDatabaseSequence> Sequences { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var mapper = new SequenceModelMapper();

            var sequenceTasks = Sequences.Select(async sequence =>
            {
                var viewModel = mapper.Map(sequence);
                var renderedSequence = Formatter.RenderTemplate(viewModel);

                var sequenceContainer = new Container(renderedSequence, Database.IdentifierDefaults.Database, "../");
                var renderedPage = Formatter.RenderTemplate(sequenceContainer);

                var outputPath = Path.Combine(ExportDirectory.FullName, sequence.Name.ToSafeKey() + ".html");
                if (!ExportDirectory.Exists)
                    ExportDirectory.Create();

                using (var writer = File.CreateText(outputPath))
                    await writer.WriteAsync(renderedPage).ConfigureAwait(false);
            });
            await Task.WhenAll(sequenceTasks).ConfigureAwait(false);
        }
    }
}
