using System;
using System.Collections.Generic;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// Splits a database identifier into the words it was written from, following the naming
/// conventions identifiers are actually written in.
/// </summary>
/// <remarks>
/// Written as a single pass over the characters rather than as a series of regular expression
/// replacements. Every name in a schema is tokenised, and a replacement-based version rewrites the
/// whole name once per boundary rule before anything is split, so the pass below exists to keep
/// the only allocations the words themselves.
/// </remarks>
internal static class NameTokenizer
{
    /// <summary>
    /// Splits a name into its lower-cased words.
    /// </summary>
    /// <param name="name">A database object name.</param>
    /// <returns>The words the name is composed of, in the order they appear. Empty when the name holds nothing but separators.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />.</exception>
    public static IReadOnlyList<string> Tokenize(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var words = new List<string>();
        var span = name.AsSpan();

        // Where the word being read began, or -1 while between words.
        var start = -1;

        for (var i = 0; i < span.Length; i++)
        {
            if (IsSeparator(span[i]))
            {
                if (start >= 0)
                {
                    words.Add(CreateWord(name, start, i - start));
                    start = -1;
                }

                continue;
            }

            if (start < 0)
            {
                start = i;
                continue;
            }

            if (IsWordBoundary(span, i))
            {
                words.Add(CreateWord(name, start, i - start));
                start = i;
            }
        }

        if (start >= 0)
            words.Add(CreateWord(name, start, span.Length - start));

        return words;
    }

    private static bool IsSeparator(char character)
    {
        return character is '_' or '-' or '.' || char.IsWhiteSpace(character);
    }

    /// <summary>
    /// Determines whether a new word begins at the given index, which is known to hold a character
    /// that is neither a separator nor the first of the word being read.
    /// </summary>
    private static bool IsWordBoundary(ReadOnlySpan<char> name, int index)
    {
        var previous = name[index - 1];
        var current = name[index];

        if (char.IsUpper(current))
        {
            // A camelCase or PascalCase boundary.
            if (char.IsLower(previous) || char.IsDigit(previous))
                return true;

            // An acronym run followed by a word, which breaks before the last upper-case character
            // of the run rather than after it: XMLHttpRequest is XML, Http, Request.
            if (char.IsUpper(previous) && index + 1 < name.Length && char.IsLower(name[index + 1]))
                return true;
        }

        // A digit run stands apart from the letters on either side of it, so that address1 yields
        // 'address' and '1' rather than a word no other name can agree with.
        return char.IsDigit(current)
            ? char.IsLetter(previous)
            : char.IsLetter(current) && char.IsDigit(previous);
    }

    // Lower-cased straight into the new string, so that a word costs one allocation whether or not
    // the name it came from was already lower case.
    private static string CreateWord(string name, int start, int length)
    {
        return string.Create(
            length,
            (Name: name, Start: start),
            static (destination, state) => state.Name.AsSpan(state.Start, destination.Length).ToLowerInvariant(destination)
        );
    }
}
