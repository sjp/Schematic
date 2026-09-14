using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Reporting.Serialization;

/// <summary>
/// Accumulates the JSON payloads produced by renderers and emits the single
/// <c>data/bundle.js</c> shim that inlines them onto <c>window.__schematic</c> so a report
/// works when opened from disk (<c>file://</c>), where <c>fetch()</c> is blocked.
/// </summary>
/// <remarks>
/// Renderers register the <c>.json</c> file they have just written rather than its contents.
/// <see cref="WriteBundleAsync"/> copies each file's bytes into the bundle, so the two sources are
/// byte-identical by construction and no payload is held in memory between rendering and
/// bundling. Renderers run concurrently, so the accumulators are thread-safe.
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
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty.</exception>
    public void AddSummary(string key, string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
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
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty.</exception>
    public void AddSummary(string key, FileInfo jsonFile)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
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
    /// <exception cref="ArgumentException"><paramref name="typeKey"/> or <paramref name="safeKey"/> is empty.</exception>
    public void AddDetail(string typeKey, string safeKey, string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(typeKey);
        ArgumentException.ThrowIfNullOrEmpty(safeKey);
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
    /// <exception cref="ArgumentException"><paramref name="typeKey"/> or <paramref name="safeKey"/> is empty.</exception>
    public void AddDetail(string typeKey, string safeKey, FileInfo jsonFile)
    {
        ArgumentException.ThrowIfNullOrEmpty(typeKey);
        ArgumentException.ThrowIfNullOrEmpty(safeKey);
        ArgumentNullException.ThrowIfNull(jsonFile);

        GetDetailMap(typeKey)[safeKey] = new FilePayload(jsonFile.FullName);
    }

    private ConcurrentDictionary<string, BundlePayload> GetDetailMap(string typeKey)
        => _details.GetOrAdd(typeKey, static _ => new ConcurrentDictionary<string, BundlePayload>(StringComparer.Ordinal));

    /// <summary>
    /// Writes the accumulated payloads to <paramref name="bundleJs"/> as a single classic
    /// script that assigns each payload onto <c>window.__schematic</c>.
    /// </summary>
    /// <remarks>
    /// The bundle is streamed to disk one payload at a time, so its size is not bounded by memory or
    /// by the maximum length of a string.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bundleJs"/> is <see langword="null" />.</exception>
    public async Task WriteBundleAsync(FileInfo bundleJs, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bundleJs);

        if (bundleJs.Directory is { Exists: false } directory)
            directory.Create();

        await using var output = new FileStream(bundleJs.FullName, new FileStreamOptions
        {
            Mode = FileMode.Create,
            Access = FileAccess.Write,
            Share = FileShare.None,
            Options = FileOptions.Asynchronous,
            BufferSize = OutputBufferSize,
        });

        await WriteTextAsync(output, "window.__schematic = window.__schematic || {};\n", cancellationToken);

        // Deterministic ordering keeps the emitted bundle reproducible across runs.
        foreach (var summary in _summaries.OrderBy(static kvp => kvp.Key, StringComparer.Ordinal))
        {
            await WriteTextAsync(output, "window.__schematic[" + EncodeKey(summary.Key) + "] = ", cancellationToken);
            await summary.Value.CopyToAsync(output, cancellationToken);
            await WriteTextAsync(output, ";\n", cancellationToken);
        }

        foreach (var typeEntry in _details.OrderBy(static kvp => kvp.Key, StringComparer.Ordinal))
        {
            var typeAccessor = "window.__schematic[" + EncodeKey(typeEntry.Key) + "]";
            await WriteTextAsync(output, typeAccessor + " = " + typeAccessor + " || {};\n", cancellationToken);

            foreach (var detail in typeEntry.Value.OrderBy(static kvp => kvp.Key, StringComparer.Ordinal))
            {
                await WriteTextAsync(output, typeAccessor + "[" + EncodeKey(detail.Key) + "] = ", cancellationToken);
                await detail.Value.CopyToAsync(output, cancellationToken);
                await WriteTextAsync(output, ";\n", cancellationToken);
            }
        }
    }

    // Below the large object heap threshold, so the stream's buffer is not allocated there.
    private const int OutputBufferSize = 64 * 1024;

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    // JSON-encode the key so it is a correctly-escaped string literal in the emitted JS.
    private static string EncodeKey(string key) => JsonSerializer.Serialize(key);

    // The script scaffolding around each payload is short, so these writes land in the output
    // stream's buffer rather than going to disk one by one.
    private static ValueTask WriteTextAsync(Stream output, string text, CancellationToken cancellationToken)
        => output.WriteAsync(Utf8NoBom.GetBytes(text), cancellationToken);

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
