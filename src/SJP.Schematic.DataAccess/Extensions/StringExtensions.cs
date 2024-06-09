using System;
using System.Globalization;
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

    /// <summary>
    /// Same as <see cref="Pascalize(string)"/> except that the first character is lower case.
    /// </summary>
    public static string Camelize(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var word = input.Pascalize();
        return word.Length > 0 ? word[..1].ToLower(CultureInfo.InvariantCulture) + word[1..] : word;
    }

    /// <summary>
    /// By default, <see cref="Pascalize(string)"/> converts strings to UpperCamelCase also removing underscores.
    /// </summary>
    public static string Pascalize(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return PascalizeRegex().Replace(input, static match => match.Groups[1].Value.ToUpper(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Separates the input words with underscore.
    /// </summary>
    /// <param name="input">The string to be underscored</param>
    public static string Underscore(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return Underscore3Regex().Replace(
            Underscore2Regex().Replace(Underscore1Regex().Replace(input, "$1_$2"), "$1_$2"),
            "_"
        ).ToLower(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Pluralizes the provided input considering irregular words.
    /// </summary>
    /// <param name="input">Word to be pluralized</param>
    public static string Pluralize(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return input + "s"; // naive, but ok for our purposes
    }
}