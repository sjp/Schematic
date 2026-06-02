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
using SJP.Schematic.Reporting.Serialization;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class LintRenderer : IDataRenderer
{
    public LintRenderer(
        IRelationalDatabaseLinter linter,
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        IReadOnlyCollection<IDatabaseView> views,
        IReadOnlyCollection<IDatabaseSequence> sequences,
        IReadOnlyCollection<IDatabaseSynonym> synonyms,
        IReadOnlyCollection<IDatabaseRoutine> routines,
        JsonDataWriter jsonWriter,
        BundleBuilder bundle,
        DirectoryInfo exportDirectory
    )
    {
        Linter = linter ?? throw new ArgumentNullException(nameof(linter));
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        Views = views ?? throw new ArgumentNullException(nameof(views));
        Sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
        Synonyms = synonyms ?? throw new ArgumentNullException(nameof(synonyms));
        Routines = routines ?? throw new ArgumentNullException(nameof(routines));
        JsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
        Bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IRelationalDatabaseLinter Linter { get; }

    private IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

    private IReadOnlyCollection<IDatabaseView> Views { get; }

    private IReadOnlyCollection<IDatabaseSequence> Sequences { get; }

    private IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; }

    private IReadOnlyCollection<IDatabaseRoutine> Routines { get; }

    private JsonDataWriter JsonWriter { get; }

    private BundleBuilder Bundle { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var (
            tableMessages,
            viewMessages,
            sequenceMessages,
            synonymMessages,
            routineMessages
        ) = await (
            Linter.AnalyseTables(Tables, cancellationToken),
            Linter.AnalyseViews(Views, cancellationToken),
            Linter.AnalyseSequences(Sequences, cancellationToken),
            Linter.AnalyseSynonyms(Synonyms, cancellationToken),
            Linter.AnalyseRoutines(Routines, cancellationToken)
        ).WhenAll();

        var messages = tableMessages
            .Concat(viewMessages)
            .Concat(sequenceMessages)
            .Concat(synonymMessages)
            .Concat(routineMessages);

        var groupedRules = messages
            .GroupAsDictionary(static m => m.RuleId)
            .Select(static m => new LintResults.LintRule(m.Value[0].Title, m.Value.ConvertAll(static r => r.Message)))
            .ToList();

        var lintVm = new LintResults(groupedRules);

        var json = JsonWriter.Serialize(lintVm);
        Bundle.AddSummary("lint", json);

        var outputFile = new FileInfo(Path.Combine(ExportDirectory.FullName, "data", "lint.json"));
        await JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
