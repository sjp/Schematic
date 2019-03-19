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
    internal sealed class SynonymRenderer : ITemplateRenderer
    {
        public SynonymRenderer(
            IRelationalDatabase database,
            IHtmlFormatter formatter,
            IReadOnlyCollection<IRelationalDatabaseTable> tables,
            IReadOnlyCollection<IDatabaseView> views,
            IReadOnlyCollection<IDatabaseSequence> sequences,
            IReadOnlyCollection<IDatabaseSynonym> synonyms,
            IReadOnlyCollection<IDatabaseRoutine> routines,
            DirectoryInfo exportDirectory
        )
        {
            if (tables == null || tables.AnyNull())
                throw new ArgumentNullException(nameof(tables));
            if (views == null || views.AnyNull())
                throw new ArgumentNullException(nameof(views));
            if (sequences == null || sequences.AnyNull())
                throw new ArgumentNullException(nameof(sequences));
            if (synonyms == null || synonyms.AnyNull())
                throw new ArgumentNullException(nameof(synonyms));
            if (routines == null || routines.AnyNull())
                throw new ArgumentNullException(nameof(routines));

            Tables = tables;
            Views = views;
            Sequences = sequences;
            Synonyms = synonyms;
            Routines = routines;

            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

            if (exportDirectory == null)
                throw new ArgumentNullException(nameof(exportDirectory));

            ExportDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "synonyms"));
        }

        private IRelationalDatabase Database { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

        private IReadOnlyCollection<IDatabaseView> Views { get; }

        private IReadOnlyCollection<IDatabaseSequence> Sequences { get; }

        private IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; }

        private IReadOnlyCollection<IDatabaseRoutine> Routines { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var mapper = new SynonymModelMapper();

            var synonymTargets = new SynonymTargets(
                Tables,
                Views,
                Sequences,
                Synonyms,
                Routines
            );
            var synonymTasks = Synonyms.Select(async synonym =>
            {
                var viewModel = mapper.Map(synonym, synonymTargets);
                var renderedSynonym = Formatter.RenderTemplate(viewModel);

                var databaseName = !Database.IdentifierDefaults.Database.IsNullOrWhiteSpace()
                    ? Database.IdentifierDefaults.Database + " Database"
                    : "Database";
                var pageTitle = synonym.Name.ToVisibleName() + " — Synonym — " + databaseName;
                var synonymContainer = new Container(renderedSynonym, pageTitle, "../");
                var renderedPage = Formatter.RenderTemplate(synonymContainer);

                var outputPath = Path.Combine(ExportDirectory.FullName, synonym.Name.ToSafeKey() + ".html");
                if (!ExportDirectory.Exists)
                    ExportDirectory.Create();

                using (var writer = File.CreateText(outputPath))
                    await writer.WriteAsync(renderedPage).ConfigureAwait(false);
            });
            await Task.WhenAll(synonymTasks).ConfigureAwait(false);
        }
    }
}
