using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Reporting.Serialization;

/// <summary>
/// Accumulates the JSON payloads produced by renderers and emits one classic script per payload
/// under <c>data/bundle/</c>, each assigning its payload onto <c>window.__schematic</c>, so a report
/// works when opened from disk (<c>file://</c>), where <c>fetch()</c> is blocked.
/// </summary>
/// <remarks>
/// <para>
/// The report loads each script only when it needs that payload, by adding a <c>&lt;script&gt;</c>
/// element, so opening a report does not download or parse the data for every page up front.
/// </para>
/// <para>
/// Renderers register the <c>.json</c> file they have just written rather than its contents.
/// <see cref="WriteBundleAsync"/> copies each file's bytes into its script, so the two sources are
/// byte-identical by construction and no payload is held in memory between rendering and
/// bundling. Renderers run concurrently, so the accumulators are thread-safe.
/// </para>
/// </remarks>
public sealed class BundleBuilder
{
    // Summary payloads register under their file stem, e.g. window.__schematic["tables"].
    private readonly ConcurrentDictionary<string, BundlePayload> _summaries = new(StringComparer.Ordinal);

    // Detail payloads register under a per-type sub-map keyed by safeKey, e.g.
    // window.__schematic["table"]["actor_a1b2c3d4"].
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, BundlePayload>> _details =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Registers a per-type summary payload under <paramref name="key"/> (the file stem, e.g.
    /// <c>tables</c>, <c>main</c>, <c>lint</c>, <c>search</c>).
    /// </summary>
    /// <remarks>
    /// The payload is kept in memory until the bundle is written. Prefer the
    /// <see cref="AddSummary(string, FileInfo)"/> overload for payloads that are already on disk.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="json"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty or is not a valid file name.</exception>
    public void AddSummary(string key, string json)
    {
        ThrowIfInvalidFileName(key);
        ArgumentNullException.ThrowIfNull(json);
        _summaries[key] = new InlinePayload(json);
    }

    /// <summary>
    /// Registers the JSON file <paramref name="jsonFile"/> as the per-type summary payload under
    /// <paramref name="key"/> (the file stem, e.g. <c>tables</c>, <c>main</c>, <c>lint</c>, <c>search</c>).
    /// </summary>
    /// <remarks>
    /// Only the file's location is retained. Its contents are read when the bundle is written, so the
    /// file must still exist, unchanged, at that point.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="jsonFile"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty or is not a valid file name.</exception>
    public void AddSummary(string key, FileInfo jsonFile)
    {
        ThrowIfInvalidFileName(key);
        ArgumentNullException.ThrowIfNull(jsonFile);
        _summaries[key] = new FilePayload(jsonFile.FullName);
    }

    /// <summary>
    /// Registers a per-object detail payload under <paramref name="typeKey"/> (e.g. <c>table</c>,
    /// <c>view</c>) keyed by <paramref name="safeKey"/>.
    /// </summary>
    /// <remarks>
    /// The payload is kept in memory until the bundle is written. Prefer the
    /// <see cref="AddDetail(string, string, FileInfo)"/> overload for payloads that are already on disk.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="typeKey"/>, <paramref name="safeKey"/> or <paramref name="json"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="typeKey"/> or <paramref name="safeKey"/> is empty or is not a valid file name.</exception>
    public void AddDetail(string typeKey, string safeKey, string json)
    {
        ThrowIfInvalidFileName(typeKey);
        ThrowIfInvalidFileName(safeKey);
        ArgumentNullException.ThrowIfNull(json);

        GetDetailMap(typeKey)[safeKey] = new InlinePayload(json);
    }

    /// <summary>
    /// Registers the JSON file <paramref name="jsonFile"/> as the per-object detail payload under
    /// <paramref name="typeKey"/> (e.g. <c>table</c>, <c>view</c>) keyed by <paramref name="safeKey"/>.
    /// </summary>
    /// <remarks>
    /// Only the file's location is retained. Its contents are read when the bundle is written, so the
    /// file must still exist, unchanged, at that point.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="typeKey"/>, <paramref name="safeKey"/> or <paramref name="jsonFile"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="typeKey"/> or <paramref name="safeKey"/> is empty or is not a valid file name.</exception>
    public void AddDetail(string typeKey, string safeKey, FileInfo jsonFile)
    {
        ThrowIfInvalidFileName(typeKey);
        ThrowIfInvalidFileName(safeKey);
        ArgumentNullException.ThrowIfNull(jsonFile);

        GetDetailMap(typeKey)[safeKey] = new FilePayload(jsonFile.FullName);
    }

    private ConcurrentDictionary<string, BundlePayload> GetDetailMap(string typeKey)
        => _details.GetOrAdd(typeKey, static _ => new ConcurrentDictionary<string, BundlePayload>(StringComparer.Ordinal));

    // Keys name the script files, so each must be a single path segment.
    private static void ThrowIfInvalidFileName(string key, [CallerArgumentExpression(nameof(key))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key, paramName);
        if (key is "." or ".." || key.AsSpan().IndexOfAny(InvalidKeyChars) >= 0)
            throw new ArgumentException("The key must be usable as a file name.", paramName);
    }

    private static readonly SearchValues<char> InvalidKeyChars = SearchValues.Create([.. Path.GetInvalidFileNameChars(), '/', '\\']);

    /// <summary>
    /// Writes each accumulated payload into <paramref name="bundleDirectory"/> as a classic script
    /// that assigns the payload onto <c>window.__schematic</c>.
    /// </summary>
    /// <remarks>
    /// A summary registered under <c>key</c> is written to <c>&lt;key&gt;.js</c> and assigned to
    /// <c>window.__schematic[key]</c>. A detail registered under <c>typeKey</c> and <c>safeKey</c> is
    /// written to <c>&lt;typeKey&gt;/&lt;safeKey&gt;.js</c> and assigned to
    /// <c>window.__schematic[typeKey][safeKey]</c>. Each script is streamed to disk, so its size is not
    /// bounded by memory or by the maximum length of a string.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bundleDirectory"/> is <see langword="null" />.</exception>
    public async Task WriteBundleAsync(DirectoryInfo bundleDirectory, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bundleDirectory);

        bundleDirectory.Create();

        var scripts = new List<BundleScript>(_summaries.Count + _details.Sum(static kvp => kvp.Value.Count));
        foreach (var (key, payload) in _summaries)
        {
            var path = Path.Combine(bundleDirectory.FullName, key + ".js");
            var prefix = RootInitializer + "window.__schematic[" + EncodeKey(key) + "] = ";
            scripts.Add(new BundleScript(path, prefix, payload));
        }

        foreach (var (typeKey, details) in _details)
        {
            var typeDirectory = Directory.CreateDirectory(Path.Combine(bundleDirectory.FullName, typeKey)).FullName;
            var typeAccessor = "window.__schematic[" + EncodeKey(typeKey) + "]";
            var typePrefix = RootInitializer + typeAccessor + " = " + typeAccessor + " || {};\n";

            foreach (var (safeKey, payload) in details)
            {
                var path = Path.Combine(typeDirectory, safeKey + ".js");
                var prefix = typePrefix + typeAccessor + "[" + EncodeKey(safeKey) + "] = ";
                scripts.Add(new BundleScript(path, prefix, payload));
            }
        }

        await Parallel.ForEachAsync(scripts, cancellationToken, static (script, ct) => new ValueTask(script.WriteAsync(ct)));
    }

    private const string RootInitializer = "window.__schematic = window.__schematic || {};\n";

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    // JSON-encode the key so it is a correctly-escaped string literal in the emitted JS.
    private static string EncodeKey(string key) => JsonSerializer.Serialize(key);

    private static readonly byte[] ScriptSuffix = ";\n"u8.ToArray();

    private sealed record BundleScript(string FilePath, string Prefix, BundlePayload Payload)
    {
        public async Task WriteAsync(CancellationToken cancellationToken)
        {
            // Unbuffered: a script is written in three parts, and the payload is copied in large
            // chunks, so a buffer per file would only add an allocation.
            await using var output = new FileStream(FilePath, new FileStreamOptions
            {
                Mode = FileMode.Create,
                Access = FileAccess.Write,
                Share = FileShare.None,
                Options = FileOptions.Asynchronous,
                BufferSize = 0,
            });

            await output.WriteAsync(Utf8NoBom.GetBytes(Prefix), cancellationToken);
            await Payload.CopyToAsync(output, cancellationToken);
            await output.WriteAsync(ScriptSuffix, cancellationToken);
        }
    }

    private abstract class BundlePayload
    {
        public abstract Task CopyToAsync(Stream output, CancellationToken cancellationToken);
    }

    // Stored as UTF-8 rather than as the caller's string: half the size, and it is the exact form
    // the payload is written in.
    private sealed class InlinePayload(string json) : BundlePayload
    {
        private readonly byte[] _utf8 = Utf8NoBom.GetBytes(json);

        public override Task CopyToAsync(Stream output, CancellationToken cancellationToken)
            => output.WriteAsync(_utf8, cancellationToken).AsTask();
    }

    private sealed class FilePayload(string path) : BundlePayload
    {
        public override async Task CopyToAsync(Stream output, CancellationToken cancellationToken)
        {
            // The copy reads in large chunks of its own, so the source stream does not need a buffer.
            await using var source = new FileStream(path, new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.Read,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
                BufferSize = 0,
            });
            await source.CopyToAsync(output, cancellationToken);
        }
    }
}
