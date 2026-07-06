using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SJP.Schematic.Lint;
using Spectre.Console;

namespace SJP.Schematic.Tool.Handlers;

internal sealed class JsonLintResultWriter : ILintResultWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public void Write(IAnsiConsole console, IReadOnlyCollection<IRuleMessage> results)
    {
        // Results have no inherent ordering -- they're gathered by iterating rules over
        // database objects -- so they're sorted here for deterministic, diff-friendly output.
        var sortedResults = results
            .OrderBy(static r => r.RuleId, StringComparer.Ordinal)
            .ThenBy(static r => r.Level)
            .ThenBy(static r => r.Message, StringComparer.Ordinal)
            .Select(static r => new LintResultDto(r.RuleId, r.Title, r.Level, r.Message))
            .ToList();

        var json = JsonSerializer.Serialize(sortedResults, SerializerOptions);

        // Write directly to the underlying writer rather than through the console's renderable
        // pipeline: WriteLine word-wraps text at the console width, which would corrupt JSON.
        console.Profile.Out.Writer.WriteLine(json);
    }

    private sealed record LintResultDto(string RuleId, string Title, RuleLevel Level, string Message);
}
