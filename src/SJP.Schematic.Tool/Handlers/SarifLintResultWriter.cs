using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using SJP.Schematic.Lint;
using SJP.Schematic.Lint.Serialization;
using Spectre.Console;

namespace SJP.Schematic.Tool.Handlers;

internal sealed class SarifLintResultWriter : ILintResultWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public void Write(IAnsiConsole console, IReadOnlyCollection<IRuleMessage> results)
    {
        var log = SarifLintReport.Create(results);
        var json = JsonSerializer.Serialize(log, SerializerOptions);

        // Write directly to the underlying writer rather than through the console's renderable
        // pipeline: WriteLine word-wraps text at the console width, which would corrupt JSON.
        console.Profile.Out.Writer.WriteLine(json);
    }
}
