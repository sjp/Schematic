using System;
using System.Collections.Generic;

namespace SJP.Schematic.Lint.Naming;

/// <summary>
/// An index of names that answers, for any one of them, which of the others are a single edit away.
/// </summary>
/// <remarks>
/// <para>
/// Each name is filed under itself and under every string formed by deleting one of its characters.
/// Two names at Damerau-Levenshtein distance 1 always share at least one of those keys, whichever
/// kind of edit separates them: an insertion or deletion because the longer name deletes to the
/// shorter one, a substitution because both delete the differing character, and a transposition
/// because <c>abcd</c> and <c>abdc</c> both delete to <c>abd</c>. Nothing is therefore missed by
/// looking only at names that share a key, which replaces comparing every pair with work linear in
/// the total number of characters.
/// </para>
/// <para>
/// A shared key does not prove the names are one edit apart — <c>ax</c> and <c>ay</c> both delete
/// to <c>a</c> — so every candidate is verified with a real distance computation before it is
/// returned.
/// </para>
/// </remarks>
internal sealed class DeletionNeighbourhoodIndex
{
    private readonly Dictionary<string, List<string>> _namesByKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletionNeighbourhoodIndex"/> class.
    /// </summary>
    /// <param name="names">The distinct names to index. Duplicates only cost repeated work; they do not change the results.</param>
    /// <exception cref="ArgumentNullException"><paramref name="names"/> is <see langword="null" />.</exception>
    public DeletionNeighbourhoodIndex(IReadOnlyCollection<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);

        // Sized from the name count rather than the key count, which is not known until the keys
        // have been built; it saves most of the growth without walking the names twice.
        _namesByKey = new Dictionary<string, List<string>>(names.Count * 2, StringComparer.Ordinal);

        foreach (var name in names)
        {
            Add(name, name);
            foreach (var key in DeletionKeys(name))
                Add(key, name);
        }
    }

    /// <summary>
    /// Retrieves the indexed names that are exactly one edit away from the given name.
    /// </summary>
    /// <param name="name">A name, which need not itself be indexed.</param>
    /// <returns>The matching names, each appearing once. The name itself is never returned.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />.</exception>
    public IReadOnlyCollection<string> GetDistanceOneMatches(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var matches = new HashSet<string>(StringComparer.Ordinal);

        Collect(name, name, matches);
        foreach (var key in DeletionKeys(name))
            Collect(key, name, matches);

        return matches;
    }

    private void Collect(string key, string name, HashSet<string> matches)
    {
        if (!_namesByKey.TryGetValue(key, out var candidates))
            return;

        foreach (var candidate in candidates)
        {
            if (string.Equals(candidate, name, StringComparison.Ordinal) || matches.Contains(candidate))
                continue;

            if (NameSimilarity.AreWithinDistanceOne(name, candidate))
                matches.Add(candidate);
        }
    }

    private static IEnumerable<string> DeletionKeys(string name)
    {
        for (var i = 0; i < name.Length; i++)
            yield return string.Concat(name.AsSpan(0, i), name.AsSpan(i + 1));
    }

    private void Add(string key, string name)
    {
        if (!_namesByKey.TryGetValue(key, out var bucket))
        {
            bucket = [];
            _namesByKey[key] = bucket;
        }

        bucket.Add(name);
    }
}
