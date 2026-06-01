using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Reporting.Serialization;

/// <summary>
/// Serializes report viewmodels to canonical JSON and writes them to <c>.json</c> files.
/// </summary>
/// <remarks>
/// Each payload is serialized exactly once via <see cref="Serialize"/>; the resulting string
/// is written to the <c>.json</c> file with <see cref="WriteJsonAsync"/> and registered with
/// the shared <see cref="BundleBuilder"/>. Producing the string once and writing it to both
/// sinks is what guarantees the <c>.json</c> files and the <c>bundle.js</c> shim cannot drift.
/// </remarks>
public sealed class JsonDataWriter
{
    // UTF-8 without a BOM: the .json files are consumed by browsers (fetch) and by the bundle
    // shim, neither of which should see a byte-order mark.
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Produces the canonical JSON string for a viewmodel. The runtime type of
    /// <paramref name="vm"/> must be registered with <see cref="ReportingJsonContext"/>.
    /// </summary>
    public string Serialize(object vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        return JsonSerializer.Serialize(vm, vm.GetType(), ReportingJsonContext.Default);
    }

    /// <summary>
    /// Writes a previously-serialized JSON string to <paramref name="file"/> as UTF-8 (no BOM),
    /// creating the parent directory if necessary.
    /// </summary>
    public async Task WriteJsonAsync(FileInfo file, string json, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(json);

        if (file.Directory is { Exists: false } directory)
            directory.Create();

        await File.WriteAllTextAsync(file.FullName, json, Utf8NoBom, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// System.Text.Json source-generation context for the reporting viewmodels: camelCase
/// property names, enums as strings, nulls omitted, not indented. Concrete viewmodel types
/// are registered with <c>[JsonSerializable(typeof(T))]</c> as renderers are converted
/// (issues 06–12).
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UseStringEnumConverter = true,
    WriteIndented = false)]
// string[] is genuinely used (lint messages). Concrete viewmodel types are registered as the
// renderer-conversion issues (06–12) convert them; nested types reachable from these roots are
// discovered automatically by the source generator.
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Main))]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Tables))]
// "TableDetail" disambiguates the top-level Table from the nested Main.Table (both simple name
// "Table"), which the source generator would otherwise map to the same TypeInfo property.
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Table), TypeInfoPropertyName = "TableDetail")]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Views))]
// "ViewDetail"/"RoutineDetail" disambiguate the top-level View/Routine from the nested
// Main.View/Main.Routine (same simple names), which the source generator would otherwise collide.
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.View), TypeInfoPropertyName = "ViewDetail")]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Routines))]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Routine), TypeInfoPropertyName = "RoutineDetail")]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Sequences))]
// "SequenceDetail"/"SynonymDetail" disambiguate the top-level Sequence/Synonym detail payloads
// from the nested Main.Sequence/Main.Synonym summary rows (same simple names).
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Sequence), TypeInfoPropertyName = "SequenceDetail")]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Synonyms))]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Synonym), TypeInfoPropertyName = "SynonymDetail")]
// Summary-only pages (issue 09): each is a schema-wide list with no per-object detail.
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Triggers))]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Columns))]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Constraints))]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Indexes))]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Orphans))]
// Lint page (issue 10): lint messages grouped by rule.
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.LintResults))]
// Relationships (issue 11): schema-wide diagram levels referencing data/diagrams/*.svg.
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Relationships))]
public partial class ReportingJsonContext : JsonSerializerContext
{
}
