using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SJP.Schematic.DataAccess.Extensions;

/// <summary>
/// Utility methods for transforming strings into different naming conventions.
/// </summary>
public static class StringExtensions
{
    private static readonly Regex _pascalizeRegex = new("(?:^|_| +)(.)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));

    private static readonly Regex _underscore1Regex = new(@"([\p{Lu}]+)([\p{Lu}][\p{Ll}])", RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));
    private static readonly Regex _underscore2Regex = new(@"([\p{Ll}\d])([\p{Lu}])", RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));
    private static readonly Regex _underscore3Regex = new(@"[-\s]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));

    /// <summary>
    /// Same as <see cref="Pascalize(string)"/> except that the first character is lower case.
    /// </summary>
    public static string Camelize(this string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        var word = input.Pascalize();
        return word.Length > 0 ? word[..1].ToLower(CultureInfo.InvariantCulture) + word[1..] : word;
    }

    /// <summary>
    /// By default, <see cref="Pascalize(string)"/> converts strings to UpperCamelCase also removing underscores.
    /// </summary>
    public static string Pascalize(this string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        return _pascalizeRegex.Replace(input, static match => match.Groups[1].Value.ToUpper(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Separates the input words with underscore.
    /// </summary>
    /// <param name="input">The string to be underscored</param>
    public static string Underscore(this string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        return _underscore3Regex.Replace(
            _underscore2Regex.Replace(_underscore1Regex.Replace(input, "$1_$2"), "$1_$2"),
            "_"
        ).ToLower(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Pluralizes the provided input considering irregular words.
    /// </summary>
    /// <param name="input">Word to be pluralized</param>
    public static string Pluralize(this string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        return input + "s"; // naive, but ok for our purposes
    }
}