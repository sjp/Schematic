using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Serialization;

/// <summary>
/// Builds a SARIF 2.1.0 log from a set of lint results.
/// </summary>
/// <remarks>
/// SARIF is the interchange format static analysis tooling already understands, so emitting it
/// lets lint results land in a code-scanning dashboard or a CI annotation without anything
/// having to parse Schematic's own output. The shape is shared between the CLI's
/// <c>--format sarif</c> output and the HTML report's <c>data/lint.sarif</c> so both describe
/// findings identically.
/// </remarks>
public static class SarifLintReport
{
    private const string SchemaUri = "https://raw.githubusercontent.com/oasis-tcs/sarif-spec/master/Schemata/sarif-schema-2.1.0.json";
    private const string InformationUri = "https://github.com/sjp/Schematic";
    private const string ToolName = "schematic";

    /// <summary>
    /// Creates a SARIF log describing <paramref name="results"/>.
    /// </summary>
    /// <param name="results">A set of lint results.</param>
    /// <returns>A SARIF log, ready to be serialized.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null" />.</exception>
    public static SarifLog Create(IEnumerable<IRuleMessage> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var resultList = results.ToList();

        // Both the rule catalogue and the result list are naturally unordered (rules are
        // discovered by iterating over database objects), so both are sorted here for
        // deterministic, diff-friendly output.
        var rules = resultList
            .GroupBy(static r => r.RuleId, StringComparer.Ordinal)
            .OrderBy(static g => g.Key, StringComparer.Ordinal)
            .Select(static g => new SarifRule(g.Key, g.First().Title, new SarifMessage(g.First().Title)))
            .ToList();

        var sarifResults = resultList
            .OrderBy(static r => r.RuleId, StringComparer.Ordinal)
            .ThenBy(static r => r.Level)
            .ThenBy(static r => r.Message, StringComparer.Ordinal)
            .Select(static r => new SarifResult(
                r.RuleId,
                ToSarifLevel(r.Level),
                new SarifMessage(r.Message),
                ToLocations(r)))
            .ToList();

        return new SarifLog(
            SchemaUri,
            "2.1.0",
            [new SarifRun(new SarifTool(new SarifDriver(ToolName, InformationUri, rules)), sarifResults)]);
    }

    /// <summary>
    /// Maps a rule level onto its SARIF equivalent.
    /// </summary>
    /// <param name="level">A rule level.</param>
    /// <returns>The SARIF level name.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="level"/> is not a known level.</exception>
    public static string ToSarifLevel(RuleLevel level) => level switch
    {
        RuleLevel.Information => "note",
        RuleLevel.Warning => "warning",
        RuleLevel.Error => "error",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown rule level."),
    };

    // A database object has no file or line, so it is described as a SARIF logical location
    // rather than a physical one. A finding with no single owning object gets no location at all,
    // which SARIF permits.
    private static IReadOnlyList<SarifLocation>? ToLocations(IRuleMessage message)
    {
        // MatchUnsafe, not Match: the None branch deliberately yields null (SARIF omits the
        // property entirely), which Match rejects.
        return message.ObjectName.MatchUnsafe(
            name => (IReadOnlyList<SarifLocation>?)
                [new SarifLocation([new SarifLogicalLocation(name.LocalName, name.ToQualifiedName())])],
            () => null);
    }
}

/// <summary>A SARIF log file.</summary>
public sealed record SarifLog(
    [property: JsonPropertyName("$schema")] string Schema,
    string Version,
    IReadOnlyList<SarifRun> Runs);

/// <summary>A single invocation of an analysis tool.</summary>
public sealed record SarifRun(SarifTool Tool, IReadOnlyList<SarifResult> Results);

/// <summary>The analysis tool that produced a run.</summary>
public sealed record SarifTool(SarifDriver Driver);

/// <summary>The analysis tool's primary component, including its rule catalogue.</summary>
public sealed record SarifDriver(string Name, string InformationUri, IReadOnlyList<SarifRule> Rules);

/// <summary>A rule in the tool's catalogue.</summary>
public sealed record SarifRule(string Id, string Name, SarifMessage ShortDescription);

/// <summary>A single finding.</summary>
public sealed record SarifResult(
    string RuleId,
    string Level,
    SarifMessage Message,
    IReadOnlyList<SarifLocation>? Locations);

/// <summary>Where a finding was detected.</summary>
public sealed record SarifLocation(IReadOnlyList<SarifLogicalLocation> LogicalLocations);

/// <summary>A location that is not a region of a file — here, a database object.</summary>
public sealed record SarifLogicalLocation(string Name, string FullyQualifiedName);

/// <summary>A human-readable string.</summary>
public sealed record SarifMessage(string Text);
