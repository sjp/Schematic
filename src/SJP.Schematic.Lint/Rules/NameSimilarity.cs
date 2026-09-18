using System;
using System.Buffers;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// String comparisons shared by the rules that look for names which are almost, but not quite,
/// the same as a name the rest of the schema agrees on.
/// </summary>
internal static class NameSimilarity
{
    // Three rolling rows of at most this many cells in total are taken from the stack. Databases
    // cap identifier length well below the resulting 127 characters per name, so the heap is only
    // reached by names that were quoted into something far longer than any dialect allows.
    private const int MaxStackAllocatedCells = 384;

    /// <summary>
    /// Determines whether two names are exactly one edit apart, counting an insertion, a deletion,
    /// a substitution or a transposition of two adjacent characters as a single edit.
    /// </summary>
    /// <param name="first">A name.</param>
    /// <param name="second">Another name.</param>
    /// <returns><see langword="true" /> if the names are at Damerau-Levenshtein distance 1; otherwise <see langword="false" />.</returns>
    public static bool AreWithinDistanceOne(string first, string second)
    {
        // A single edit changes a name's length by at most one, so this rejects most pairs before
        // the distance is computed at all.
        if (Math.Abs(first.Length - second.Length) > 1)
            return false;

        return DamerauLevenshteinDistance(first, second) == 1;
    }

    /// <summary>
    /// Computes the restricted Damerau-Levenshtein distance (optimal string alignment) between two names.
    /// </summary>
    /// <param name="source">A name.</param>
    /// <param name="target">Another name.</param>
    /// <returns>The number of insertions, deletions, substitutions and adjacent transpositions needed to turn one into the other.</returns>
    public static int DamerauLevenshteinDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
    {
        if (source.Length == 0)
            return target.Length;
        if (target.Length == 0)
            return source.Length;

        var width = target.Length + 1;

        // Transpositions read the row two above the current one, so three rows are kept rather
        // than the two a plain Levenshtein distance would need.
        int[]? rented = null;
        var buffer = width * 3 <= MaxStackAllocatedCells
            ? stackalloc int[MaxStackAllocatedCells]
            : (rented = ArrayPool<int>.Shared.Rent(width * 3)).AsSpan();

        try
        {
            var twoRowsBack = buffer[..width];
            var oneRowBack = buffer.Slice(width, width);
            var currentRow = buffer.Slice(width * 2, width);

            for (var j = 0; j < width; j++)
                oneRowBack[j] = j;

            for (var i = 1; i <= source.Length; i++)
            {
                currentRow[0] = i;

                for (var j = 1; j <= target.Length; j++)
                {
                    var substitutionCost = source[i - 1] == target[j - 1] ? 0 : 1;
                    var cost = Math.Min(
                        Math.Min(oneRowBack[j] + 1, currentRow[j - 1] + 1),
                        oneRowBack[j - 1] + substitutionCost
                    );

                    if (i > 1 && j > 1 && source[i - 1] == target[j - 2] && source[i - 2] == target[j - 1])
                        cost = Math.Min(cost, twoRowsBack[j - 2] + 1);

                    currentRow[j] = cost;
                }

                var completedRow = oneRowBack;
                oneRowBack = currentRow;
                currentRow = twoRowsBack;
                twoRowsBack = completedRow;
            }

            return oneRowBack[target.Length];
        }
        finally
        {
            if (rented != null)
                ArrayPool<int>.Shared.Return(rented);
        }
    }

    /// <summary>
    /// Determines whether two names differ only by a trailing <c>s</c>, as a singular and its plural do.
    /// </summary>
    /// <param name="first">A name.</param>
    /// <param name="second">Another name.</param>
    /// <returns><see langword="true" /> if one name is the other with an <c>s</c> appended; otherwise <see langword="false" />.</returns>
    /// <remarks>
    /// The <c>es</c> plural of a stem ending in a sibilant (<c>address</c> against <c>addresses</c>)
    /// is two edits apart, so it is already outside the distance-1 pairs this is asked about and
    /// needs no case of its own here.
    /// </remarks>
    public static bool DiffersOnlyByTrailingPluralS(string first, string second)
    {
        var (shorter, longer) = first.Length <= second.Length ? (first, second) : (second, first);
        if (longer.Length != shorter.Length + 1)
            return false;

        return (longer[^1] == 's' || longer[^1] == 'S')
            && longer.AsSpan(0, shorter.Length).Equals(shorter, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether two names are the same once their digits are removed, as <c>address1</c>
    /// and <c>address2</c> are.
    /// </summary>
    /// <param name="first">A name.</param>
    /// <param name="second">Another name.</param>
    /// <returns><see langword="true" /> if the names agree on every non-digit character, in order; otherwise <see langword="false" />.</returns>
    public static bool DiffersOnlyByDigits(string first, string second)
    {
        var firstIndex = 0;
        var secondIndex = 0;

        while (true)
        {
            while (firstIndex < first.Length && char.IsDigit(first[firstIndex]))
                firstIndex++;
            while (secondIndex < second.Length && char.IsDigit(second[secondIndex]))
                secondIndex++;

            if (firstIndex == first.Length || secondIndex == second.Length)
                return firstIndex == first.Length && secondIndex == second.Length;

            if (first[firstIndex] != second[secondIndex])
                return false;

            firstIndex++;
            secondIndex++;
        }
    }
}
