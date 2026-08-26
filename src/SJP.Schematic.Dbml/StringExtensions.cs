using System;
using System.Buffers;
using System.Text.RegularExpressions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Dbml;

internal static partial class StringExtensions
{
    private static readonly SearchValues<char> EscapableChars = SearchValues.Create(['\\', '\'', '"']);

    public static string RemoveEnclosingQuotingCharacters(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Length < 2)
            return input;

        var isQuoted = (input[0], input[^1]) switch
        {
            ('"', '"') => true,
            ('\'', '\'') => true,
            ('`', '`') => true,
            ('[', ']') => true,
            _ => false
        };

        return isQuoted ? input[1..^1] : input;
    }

    public static string ToDbmlIdentifier(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return SafeIdentifierRegex().IsMatch(input)
            ? input
            : "\"" + Escape(input, '"') + "\"";
    }

    public static string ToDbmlTypeName(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return SafeTypeNameRegex().IsMatch(input)
            ? input
            : "\"" + Escape(input, '"') + "\"";
    }

    public static string ToDbmlStringLiteral(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return "'" + Escape(input, '\'') + "'";
    }

    public static string ToDbmlExpression(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return "`" + input + "`";
    }

    private static string Escape(string input, char quoteChar)
    {
        // fast path
        if (!input.AsSpan().ContainsAny(EscapableChars))
            return input;

        var builder = StringBuilderCache.Acquire(input.Length);

        foreach (var c in input)
        {
            if (c == '\\' || c == quoteChar)
                builder.Append('\\');
            builder.Append(c);
        }

        return builder.GetStringAndRelease();
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex SafeIdentifierRegex();

    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]*(\([A-Za-z0-9_, ]*\))?$", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex SafeTypeNameRegex();
}
