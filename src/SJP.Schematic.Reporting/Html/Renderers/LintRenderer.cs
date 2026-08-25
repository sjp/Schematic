using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;
using SJP.Schematic.Lint.Serialization;
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

        // Which analysis produced a message is what identifies the kind of object it belongs to,
        // so the object type is captured here rather than being carried on IRuleMessage.
        var messages = ToViewModels(tableMessages, LintObjectType.Table)
            .Concat(ToViewModels(viewMessages, LintObjectType.View))
            .Concat(ToViewModels(sequenceMessages, LintObjectType.Sequence))
            .Concat(ToViewModels(synonymMessages, LintObjectType.Synonym))
            .Concat(ToViewModels(routineMessages, LintObjectType.Routine))
            // Order by rule id, then by the message itself, so lint.json (and the bundle) is
            // reproducible across runs; analysis order is otherwise unspecified.
            .OrderBy(static m => m.RuleId, StringComparer.Ordinal)
            .ThenBy(static m => m.Message, StringComparer.Ordinal)
            .ToList();

        var rules = messages
            .GroupBy(static m => m.RuleId, StringComparer.Ordinal)
            .OrderBy(static g => g.Key, StringComparer.Ordinal)
            .Select(static g => new LintResults.LintRule(
                g.Key,
                g.First().RuleTitle,
                // Every message from a rule carries that rule's level, so the first is representative.
                g.First().Level,
                (uint)g.Count()))
            .ToList();

        var lintVm = new LintResults(rules, messages);

        var json = context.JsonWriter.Serialize(lintVm);
        context.Bundle.AddSummary("lint", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "lint.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);

        await WriteSarifAsync(
            tableMessages.Concat(viewMessages).Concat(sequenceMessages).Concat(synonymMessages).Concat(routineMessages),
            context,
            cancellationToken).ConfigureAwait(false);
    }

    // Emitted alongside lint.json so a generated report can be fed straight to code-scanning
    // tooling. Deliberately not registered with the bundle: nothing in the UI reads it, and the
    // bundle exists only so the SPA can load its data from disk.
    private static async Task WriteSarifAsync(IEnumerable<IRuleMessage> messages, RenderContext context, CancellationToken cancellationToken)
    {
        var sarif = SarifLintReport.Create(messages);
        var sarifJson = context.JsonWriter.Serialize(sarif);

        var sarifFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "lint.sarif"));
        await context.JsonWriter.WriteJsonAsync(sarifFile, sarifJson, cancellationToken).ConfigureAwait(false);
    }

    private static IEnumerable<LintResults.LintMessage> ToViewModels(IEnumerable<IRuleMessage> messages, LintObjectType objectType)
    {
        return messages.Select(m => new LintResults.LintMessage(
            m.RuleId,
            m.Title,
            m.Level,
            m.Message,
            m.ObjectName.MatchUnsafe(static name => name, () => (Identifier?)null),
            objectType));
    }
}
