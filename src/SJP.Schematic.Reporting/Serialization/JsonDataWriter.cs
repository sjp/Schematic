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
/// Each payload is serialized exactly once, straight into its <c>.json</c> file via
/// <see cref="SerializeToFileAsync"/>, and that file is then registered with the shared
/// <see cref="BundleBuilder"/>, which copies its bytes into a script for opening the report from
/// disk. Writing the payload once and reusing the file's bytes is what guarantees the <c>.json</c>
/// files and those scripts cannot drift, without holding any payload in memory.
/// </remarks>
public sealed class JsonDataWriter
{
    // UTF-8 without a BOM: the .json files are consumed by browsers (fetch) and by the bundle
    // shim, neither of which should see a byte-order mark.
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Serializes a viewmodel as canonical JSON directly into <paramref name="file"/> as UTF-8
    /// (no BOM), creating the parent directory if necessary and replacing any existing file. The
    /// runtime type of <paramref name="vm"/> must be registered with <see cref="ReportingJsonContext"/>.
    /// </summary>
    /// <remarks>
    /// The output is byte-for-byte the UTF-8 encoding of <see cref="Serialize"/>, but the payload
    /// is streamed to disk in chunks rather than materialized as a string first.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="file"/> or <paramref name="vm"/> is <see langword="null" />.</exception>
    public async Task SerializeToFileAsync(FileInfo file, object vm, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(vm);

        await using var stream = OpenForWrite(file);
        await JsonSerializer.SerializeAsync(stream, vm, vm.GetType(), ReportingJsonContext.Default, cancellationToken);
    }

    private static FileStream OpenForWrite(FileInfo file)
    {
        // The serializer buffers its own output, so the file stream does not need a second buffer.
        var options = new FileStreamOptions
        {
            Mode = FileMode.Create,
            Access = FileAccess.Write,
            Share = FileShare.None,
            Options = FileOptions.Asynchronous,
            BufferSize = 0,
        };

        try
        {
            return new FileStream(file.FullName, options);
        }
        catch (IOException ex) when (IsMissingDirectory(ex) && file.DirectoryName != null)
        {
            // Many files share each directory, so the directory is created only when opening a
            // file finds it missing rather than checked for before every write.
            Directory.CreateDirectory(file.DirectoryName);
            return new FileStream(file.FullName, options);
        }
    }

    // Creating a file only fails as "not found" when its directory is missing. That is reported as
    // either exception type: when another writer creates the directory between the failed open and
    // the runtime checking why it failed, the runtime reports the file rather than the directory.
    private static bool IsMissingDirectory(IOException ex) => ex is DirectoryNotFoundException or FileNotFoundException;

    /// <summary>
    /// Produces the canonical JSON string for a viewmodel. The runtime type of
    /// <paramref name="vm"/> must be registered with <see cref="ReportingJsonContext"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="vm"/> is <see langword="null" />.</exception>
    public string Serialize(object vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        return JsonSerializer.Serialize(vm, vm.GetType(), ReportingJsonContext.Default);
    }

    /// <summary>
    /// Writes a previously-serialized JSON string to <paramref name="file"/> as UTF-8 (no BOM),
    /// creating the parent directory if necessary.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="file"/> or <paramref name="json"/> is <see langword="null" />.</exception>
    public async Task WriteJsonAsync(FileInfo file, string json, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            await File.WriteAllTextAsync(file.FullName, json, Utf8NoBom, cancellationToken);
        }
        catch (IOException ex) when (IsMissingDirectory(ex) && file.DirectoryName != null)
        {
            // The file cannot have been opened, so nothing was written and the write can be retried.
            Directory.CreateDirectory(file.DirectoryName);
            await File.WriteAllTextAsync(file.FullName, json, Utf8NoBom, cancellationToken);
        }
    }
}

/// <summary>
/// System.Text.Json source-generation context for the reporting viewmodels: camelCase
/// property names, enums as strings, nulls omitted, not indented. Concrete viewmodel types
/// are registered with <c>[JsonSerializable(typeof(T))]</c>.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UseStringEnumConverter = true,
    WriteIndented = false)]
// string[] is genuinely used (lint messages). Nested types reachable from these roots are
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
// Schemas & the types declared within them. "SchemaDetail"/"UserDefinedTypeDetail" disambiguate the
// top-level detail payloads from the nested Main.Schema/Main.UserDefinedType summary rows.
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Schemas))]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Schema), TypeInfoPropertyName = "SchemaDetail")]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.UserDefinedTypes))]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.UserDefinedType), TypeInfoPropertyName = "UserDefinedTypeDetail")]
// Summary-only pages: each is a schema-wide list with no per-object detail.
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Triggers))]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Columns))]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Constraints))]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Indexes))]
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Orphans))]
// Lint page: the rule catalogue plus a flat list of every message.
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.LintResults))]
// data/lint.sarif: the same findings in the interchange format code-scanning tools read.
[JsonSerializable(typeof(SJP.Schematic.Lint.Serialization.SarifLog))]
// Relationships: the schema-wide table graph, also the source of each table page's diagrams.
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Relationships))]
// Search: flat index of every object + column for the Cmd/Ctrl-K palette.
[JsonSerializable(typeof(SJP.Schematic.Reporting.Html.ViewModels.Search))]
public partial class ReportingJsonContext : JsonSerializerContext
{
}
