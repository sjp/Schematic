using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class LintRenderer : ITemplateRenderer
    {
        public LintRenderer(
            IRelationalDatabaseLinter linter,
            IIdentifierDefaults identifierDefaults,
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

            Linter = linter ?? throw new ArgumentNullException(nameof(linter));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IRelationalDatabaseLinter Linter { get; }

        private IIdentifierDefaults IdentifierDefaults { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

        private IReadOnlyCollection<IDatabaseView> Views { get; }

        private IReadOnlyCollection<IDatabaseSequence> Sequences { get; }

        private IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; }

        private IReadOnlyCollection<IDatabaseRoutine> Routines { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default)
        {
            var tableMessages = await Linter.AnalyseTables(Tables, cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var viewMessages = await Linter.AnalyseViews(Views, cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var sequenceMessages = await Linter.AnalyseSequences(Sequences, cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var synonymMessages = await Linter.AnalyseSynonyms(Synonyms, cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var routineMessages = await Linter.AnalyseRoutines(Routines, cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);

            var messages = tableMessages
                .Concat(viewMessages)
                .Concat(sequenceMessages)
                .Concat(synonymMessages)
                .Concat(routineMessages);

            var groupedRules = messages
                .GroupBy(static m => m.RuleId, StringComparer.Ordinal)
                .Select(static m => new LintResults.LintRule(m.First().Title, m.Select(static r => new HtmlString(r.Message)).ToList()))
                .ToList();

            var templateParameter = new LintResults(groupedRules);
            var renderedLint = await Formatter.RenderTemplateAsync(templateParameter, cancellationToken).ConfigureAwait(false);

            var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? IdentifierDefaults.Database + " Database"
                : "Database";
            var pageTitle = "Lint · " + databaseName;
            var lintContainer = new Container(renderedLint, pageTitle, string.Empty);
            var renderedPage = await Formatter.RenderTemplateAsync(lintContainer, cancellationToken).ConfigureAwait(false);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "lint.html");

            using var writer = File.CreateText(outputPath);
            await writer.WriteAsync(renderedPage.AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
        }
    }
}
