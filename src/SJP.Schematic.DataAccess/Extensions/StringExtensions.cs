using System;
using System.Text.RegularExpressions;

namespace SJP.Schematic.DataAccess.Extensions
{
    /// <summary>
    /// Utility methods for transforming strings into different naming conventions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Same as <see cref="Pascalize(string)"/> except that the first character is lower case.
        /// </summary>
        public static string Camelize(this string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var word = input.Pascalize();
            return word.Length > 0 ? word.Substring(0, 1).ToLower() + word.Substring(1) : word;
        }

        /// <summary>
        /// By default, <see cref="Pascalize(string)"/> converts strings to UpperCamelCase also removing underscores.
        /// </summary>
        public static string Pascalize(this string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return Regex.Replace(input, "(?:^|_| +)(.)", match => match.Groups[1].Value.ToUpper());
        }

        /// <summary>
        /// Separates the input words with underscore.
        /// </summary>
        /// <param name="input">The string to be underscored</param>
        public static string Underscore(this string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return Regex.Replace(
                Regex.Replace(
                    Regex.Replace(input, @"([\p{Lu}]+)([\p{Lu}][\p{Ll}])", "$1_$2"), @"([\p{Ll}\d])([\p{Lu}])", "$1_$2"), @"[-\s]", "_").ToLower();
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
}
