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

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class LintRenderer : ITemplateRenderer
{
    public LintRenderer(
        IRelationalDatabaseLinter linter,
        IIdentifierDefaults identifierDefaults,
        IHtmlFormatter formatter,
        IEnumerable<IRelationalDatabaseTable> tables,
        IEnumerable<IDatabaseView> views,
        IEnumerable<IDatabaseSequence> sequences,
        IEnumerable<IDatabaseSynonym> synonyms,
        IEnumerable<IDatabaseRoutine> routines,
        DirectoryInfo exportDirectory
    )
    {
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        Views = views ?? throw new ArgumentNullException(nameof(views));
        Sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
        Synonyms = synonyms ?? throw new ArgumentNullException(nameof(synonyms));
        Routines = routines ?? throw new ArgumentNullException(nameof(routines));

        Linter = linter ?? throw new ArgumentNullException(nameof(linter));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IRelationalDatabaseLinter Linter { get; }

    private IIdentifierDefaults IdentifierDefaults { get; }

    private IHtmlFormatter Formatter { get; }

    private IEnumerable<IRelationalDatabaseTable> Tables { get; }

    private IEnumerable<IDatabaseView> Views { get; }

    private IEnumerable<IDatabaseSequence> Sequences { get; }

    private IEnumerable<IDatabaseSynonym> Synonyms { get; }

    private IEnumerable<IDatabaseRoutine> Routines { get; }

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
            .GroupAsDictionary(static m => m.RuleId)
            .Select(static m => new LintResults.LintRule(m.Value[0].Title, m.Value.ConvertAll(static r => new HtmlString(r.Message))))
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
