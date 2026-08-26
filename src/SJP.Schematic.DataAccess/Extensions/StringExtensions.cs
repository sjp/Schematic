using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SJP.Schematic.DataAccess.Extensions;

/// <summary>
/// Utility methods for transforming strings into different naming conventions.
/// </summary>
public static partial class StringExtensions
{
    [GeneratedRegex("(?:^|_| +)(.)", RegexOptions.Compiled, matchTimeoutMilliseconds: 200)]
    private static partial Regex PascalizeRegex();

    [GeneratedRegex(@"([\p{Lu}]+)([\p{Lu}][\p{Ll}])", RegexOptions.Compiled, matchTimeoutMilliseconds: 200)]
    private static partial Regex Underscore1Regex();

    [GeneratedRegex(@"([\p{Ll}\d])([\p{Lu}])", RegexOptions.Compiled, matchTimeoutMilliseconds: 200)]
    private static partial Regex Underscore2Regex();

    [GeneratedRegex(@"[-\s]", RegexOptions.Compiled, matchTimeoutMilliseconds: 200)]
    private static partial Regex Underscore3Regex();

    private static readonly string[] SibilantEndings = ["s", "x", "z", "ch", "sh"];

    private static readonly char[] Vowels = ['a', 'e', 'i', 'o', 'u'];

    private static readonly FrozenSet<string> UncountableWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "equipment",
        "information",
        "money",
        "news",
        "series",
        "sheep",
        "software",
        "species",
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, string> IrregularPlurals = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["child"] = "children",
        ["foot"] = "feet",
        ["goose"] = "geese",
        ["life"] = "lives",
        ["man"] = "men",
        ["mouse"] = "mice",
        ["person"] = "people",
        ["tooth"] = "teeth",
        ["woman"] = "women",
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Same as <see cref="Pascalize(string)"/> except that the first character is lower case.
    /// </summary>
    /// <param name="input">The string to be camelized.</param>
    /// <returns>A camel-cased string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null" />.</exception>
    public static string Camelize(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var word = input.Pascalize();
        return word.Length > 0 ? word[..1].ToLower(CultureInfo.InvariantCulture) + word[1..] : word;
    }

    /// <summary>
    /// By default, <see cref="Pascalize(string)"/> converts strings to UpperCamelCase also removing underscores.
    /// </summary>
    /// <param name="input">The string to be pascalized.</param>
    /// <returns>A pascal-cased string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null" />.</exception>
    public static string Pascalize(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return PascalizeRegex().Replace(input, static match => match.Groups[1].Value.ToUpper(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Separates the input words with underscore.
    /// </summary>
    /// <param name="input">The string to be underscored</param>
    /// <returns>An underscored string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null" />.</exception>
    public static string Underscore(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return Underscore3Regex().Replace(
            Underscore2Regex().Replace(Underscore1Regex().Replace(input, "$1_$2"), "$1_$2"),
            "_"
        ).ToLower(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Pluralizes the provided input. Applies regular English suffix rules to the word as a whole,
    /// with a small set of irregular and uncountable words handled by lookup. Compound words are
    /// not analysed, so the result is a best-effort approximation rather than a linguistically
    /// accurate plural.
    /// </summary>
    /// <param name="input">Word to be pluralized</param>
    /// <returns>A pluralized word.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null" />.</exception>
    public static string Pluralize(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Length == 0)
            return input;

        if (UncountableWords.Contains(input))
            return input;

        if (IrregularPlurals.TryGetValue(input, out var irregularPlural))
            return MatchCase(input, irregularPlural);

        // an all-caps word should not gain a lower-cased suffix, e.g. 'ADDRESS' -> 'ADDRESSES'
        var isUpperCase = input.Any(char.IsLetter) && !input.Any(char.IsLower);

        // words ending in '-sis' replace it with '-ses', e.g. 'analysis' -> 'analyses'
        if (input.Length > 3 && input.EndsWith("sis", StringComparison.OrdinalIgnoreCase))
            return input[..^2] + Suffix("es", isUpperCase);

        // sibilant endings require an extra syllable, e.g. 'box' -> 'boxes'
        if (EndsWithAny(input, SibilantEndings))
            return input + Suffix("es", isUpperCase);

        // a consonant followed by 'y' becomes '-ies', e.g. 'category' -> 'categories'
        if (input.Length > 1 && (input[^1] is 'y' or 'Y') && !IsVowel(input[^2]))
            return input[..^1] + Suffix("ies", isUpperCase);

        return input + Suffix("s", isUpperCase);
    }

    private static string Suffix(string suffix, bool isUpperCase) => isUpperCase ? suffix.ToUpper(CultureInfo.InvariantCulture) : suffix;

    private static bool EndsWithAny(string input, IEnumerable<string> endings) => endings.Any(e => input.EndsWith(e, StringComparison.OrdinalIgnoreCase));

    private static bool IsVowel(char c) => Vowels.Contains(char.ToLower(c, CultureInfo.InvariantCulture));

    private static string MatchCase(string input, string plural)
    {
        if (!input.Any(char.IsLower))
            return plural.ToUpper(CultureInfo.InvariantCulture);

        return char.IsUpper(input[0])
            ? plural[..1].ToUpper(CultureInfo.InvariantCulture) + plural[1..]
            : plural;
    }
}