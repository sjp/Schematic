using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class LintRenderer : IDataRenderer
{
    public LintRenderer(IRelationalDatabaseLinter linter)
    {
        Linter = linter ?? throw new ArgumentNullException(nameof(linter));
    }

    private IRelationalDatabaseLinter Linter { get; }

    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var (
            tableMessages,
            viewMessages,
            sequenceMessages,
            synonymMessages,
            routineMessages
        ) = await (
            Linter.AnalyseTables(data.Tables, cancellationToken),
            Linter.AnalyseViews(data.Views, cancellationToken),
            Linter.AnalyseSequences(data.Sequences, cancellationToken),
            Linter.AnalyseSynonyms(data.Synonyms, cancellationToken),
            Linter.AnalyseRoutines(data.Routines, cancellationToken)
        ).WhenAll();

        var messages = tableMessages
            .Concat(viewMessages)
            .Concat(sequenceMessages)
            .Concat(synonymMessages)
            .Concat(routineMessages);

        var groupedRules = messages
            .GroupAsDictionary(static m => m.RuleId)
            // Order by rule id so lint.json (and the bundle) is reproducible across runs; dictionary
            // iteration order is otherwise unspecified.
            .OrderBy(static m => m.Key, StringComparer.Ordinal)
            .Select(static m => new LintResults.LintRule(m.Value[0].Title, m.Value.ConvertAll(static r => r.Message)))
            .ToList();

        var lintVm = new LintResults(groupedRules);

        var json = context.JsonWriter.Serialize(lintVm);
        context.Bundle.AddSummary("lint", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "lint.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
