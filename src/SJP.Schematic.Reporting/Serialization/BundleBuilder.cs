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
/// The values written here are the exact same serialized strings written to the <c>.json</c>
/// files, so the two sources are byte-identical by construction. Renderers run concurrently,
/// so the accumulators are thread-safe.
/// </remarks>
public sealed class BundleBuilder
{
    // Summary payloads register under their file stem, e.g. window.__schematic["tables"].
    private readonly ConcurrentDictionary<string, string> _summaries = new(StringComparer.Ordinal);

    // Detail payloads register under a per-type sub-map keyed by safeKey, e.g.
    // window.__schematic["table"]["actor_a1b2c3d4"].
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _details =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Registers a per-type summary payload under <paramref name="key"/> (the file stem, e.g.
    /// <c>tables</c>, <c>main</c>, <c>lint</c>, <c>search</c>).
    /// </summary>
    public void AddSummary(string key, string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(json);
        _summaries[key] = json;
    }

    /// <summary>
    /// Registers a per-object detail payload under <paramref name="typeKey"/> (e.g. <c>table</c>,
    /// <c>view</c>) keyed by <paramref name="safeKey"/>.
    /// </summary>
    public void AddDetail(string typeKey, string safeKey, string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(typeKey);
        ArgumentException.ThrowIfNullOrEmpty(safeKey);
        ArgumentNullException.ThrowIfNull(json);

        var map = _details.GetOrAdd(typeKey, static _ => new ConcurrentDictionary<string, string>(StringComparer.Ordinal));
        map[safeKey] = json;
    }

    /// <summary>
    /// Writes the accumulated payloads to <paramref name="bundleJs"/> as a single classic
    /// script that assigns each payload onto <c>window.__schematic</c>.
    /// </summary>
    public async Task WriteBundleAsync(FileInfo bundleJs, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bundleJs);

        if (bundleJs.Directory is { Exists: false } directory)
            directory.Create();

        var builder = new StringBuilder();
        builder.Append("window.__schematic = window.__schematic || {};\n");

        // Deterministic ordering keeps the emitted bundle reproducible across runs.
        foreach (var summary in _summaries.OrderBy(static kvp => kvp.Key, StringComparer.Ordinal))
        {
            builder.Append("window.__schematic[").Append(EncodeKey(summary.Key)).Append("] = ")
                .Append(summary.Value).Append(";\n");
        }

        foreach (var typeEntry in _details.OrderBy(static kvp => kvp.Key, StringComparer.Ordinal))
        {
            var encodedType = EncodeKey(typeEntry.Key);
            builder.Append("window.__schematic[").Append(encodedType).Append("] = window.__schematic[")
                .Append(encodedType).Append("] || {};\n");

            foreach (var detail in typeEntry.Value.OrderBy(static kvp => kvp.Key, StringComparer.Ordinal))
            {
                builder.Append("window.__schematic[").Append(encodedType).Append("][")
                    .Append(EncodeKey(detail.Key)).Append("] = ").Append(detail.Value).Append(";\n");
            }
        }

        await File.WriteAllTextAsync(bundleJs.FullName, builder.ToString(), Utf8NoBom, cancellationToken).ConfigureAwait(false);
    }

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    // JSON-encode the key so it is a correctly-escaped string literal in the emitted JS.
    private static string EncodeKey(string key) => JsonSerializer.Serialize(key);
}
