using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SJP.Schematic.Lint;
using Spectre.Console;

namespace SJP.Schematic.Tool.Handlers;

internal sealed class SarifLintResultWriter : ILintResultWriter
{
    private const string SchemaUri = "https://raw.githubusercontent.com/oasis-tcs/sarif-spec/master/Schemata/sarif-schema-2.1.0.json";
    private const string InformationUri = "https://github.com/sjp/Schematic";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public void Write(IAnsiConsole console, IReadOnlyCollection<IRuleMessage> results)
    {
        // Both the rule catalogue and the result list are naturally unordered (rules are
        // discovered by iterating over database objects), so both are sorted here for
        // deterministic, diff-friendly output.
        var rules = results
            .GroupBy(static r => r.RuleId, StringComparer.Ordinal)
            .OrderBy(static g => g.Key, StringComparer.Ordinal)
            .Select(static g => new SarifRule(g.Key, g.First().Title, new SarifMessage(g.First().Title)))
            .ToList();

        var sarifResults = results
            .OrderBy(static r => r.RuleId, StringComparer.Ordinal)
            .ThenBy(static r => r.Level)
            .ThenBy(static r => r.Message, StringComparer.Ordinal)
            .Select(static r => new SarifResult(r.RuleId, ToSarifLevel(r.Level), new SarifMessage(r.Message)))
            .ToList();

        var log = new SarifLog(
            SchemaUri,
            "2.1.0",
            [new SarifRun(new SarifTool(new SarifDriver("schematic", InformationUri, rules)), sarifResults)]);

        var json = JsonSerializer.Serialize(log, SerializerOptions);

        // Write directly to the underlying writer rather than through the console's renderable
        // pipeline: WriteLine word-wraps text at the console width, which would corrupt JSON.
        console.Profile.Out.Writer.WriteLine(json);
    }

    private static string ToSarifLevel(RuleLevel level) => level switch
    {
        RuleLevel.Information => "note",
        RuleLevel.Warning => "warning",
        RuleLevel.Error => "error",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown rule level."),
    };

    private sealed record SarifLog(
        [property: JsonPropertyName("$schema")] string Schema,
        string Version,
        IReadOnlyList<SarifRun> Runs);

    private sealed record SarifRun(SarifTool Tool, IReadOnlyList<SarifResult> Results);

    private sealed record SarifTool(SarifDriver Driver);

    private sealed record SarifDriver(string Name, string InformationUri, IReadOnlyList<SarifRule> Rules);

    private sealed record SarifRule(string Id, string Name, SarifMessage ShortDescription);

    private sealed record SarifResult(string RuleId, string Level, SarifMessage Message);

    private sealed record SarifMessage(string Text);
}
