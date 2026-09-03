using System;
using System.Buffers;
using System.Text.RegularExpressions;
using SJP.Schematic.Core;
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

    public static string ToDbmlDefaultValue(this IDatabaseDefaultValue defaultValue)
    {
        ArgumentNullException.ThrowIfNull(defaultValue);

        var value = RemoveEnclosingParentheses(defaultValue.Definition.Trim());
        if (value.Length == 0)
            return "''";

        return defaultValue.Kind switch
        {
            DefaultValueKind.Null => "null",
            DefaultValueKind.Literal => ToDbmlLiteralValue(value),
            DefaultValueKind.Expression or DefaultValueKind.SequenceNextValue => ToDbmlExpressionValue(value),
            // a dialect that could not classify the default leaves DBML to guess from its shape
            _ => ToDbmlUnclassifiedValue(value),
        };
    }

    private static string ToDbmlLiteralValue(string value)
    {
        if (IsDbmlKeyword(value))
            return value.ToLowerInvariant();

        if (NumericLiteralRegex().IsMatch(value))
            return value;

        // a dialect that reports the value of a literal rather than the SQL that produced it, i.e.
        // MySQL, hands over the text of the value itself rather than a quoted literal
        return TryGetSqlStringLiteral(value, out var literal)
            ? literal.ToDbmlStringLiteral()
            : value.ToDbmlStringLiteral();
    }

    private static string ToDbmlExpressionValue(string value)
    {
        // an expression is delimited by backticks, so one that contains a backtick
        // can only be preserved as a string
        return value.Contains('`', StringComparison.Ordinal)
            ? value.ToDbmlStringLiteral()
            : value.ToDbmlExpression();
    }

    private static string ToDbmlUnclassifiedValue(string value)
    {
        if (IsDbmlKeyword(value))
            return value.ToLowerInvariant();

        if (NumericLiteralRegex().IsMatch(value))
            return value;

        if (TryGetSqlStringLiteral(value, out var literal))
            return literal.ToDbmlStringLiteral();

        return ToDbmlExpressionValue(value);
    }

    private static bool IsDbmlKeyword(string input)
    {
        return string.Equals(input, "null", StringComparison.OrdinalIgnoreCase)
            || string.Equals(input, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(input, "false", StringComparison.OrdinalIgnoreCase);
    }

    private static string RemoveEnclosingParentheses(string input)
    {
        var result = input;

        while (result.Length > 1 && result[0] == '(' && result[^1] == ')' && IsEnclosedByOuterParentheses(result))
            result = result[1..^1].Trim();

        return result;
    }

    private static bool IsEnclosedByOuterParentheses(string input)
    {
        var depth = 0;
        var inStringLiteral = false;

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (inStringLiteral)
            {
                if (c == '\'')
                    inStringLiteral = false;
                continue;
            }

            switch (c)
            {
                case '\'':
                    inStringLiteral = true;
                    break;
                case '(':
                    depth++;
                    break;
                case ')':
                    depth--;
                    if (depth == 0 && i < input.Length - 1)
                        return false;
                    break;
            }
        }

        return depth == 0;
    }

    private static bool TryGetSqlStringLiteral(string input, out string value)
    {
        value = string.Empty;

        // national character literals (e.g. N'test') carry the same text
        var literal = input.Length > 2 && (input[0] == 'N' || input[0] == 'n') && input[1] == '\''
            ? input[1..]
            : input;

        if (literal.Length < 2 || literal[0] != '\'' || literal[^1] != '\'')
            return false;

        var builder = StringBuilderCache.Acquire(literal.Length);
        var isSingleLiteral = true;

        for (var i = 1; i < literal.Length - 1; i++)
        {
            var c = literal[i];
            if (c != '\'')
            {
                builder.Append(c);
                continue;
            }

            // an embedded quote must be doubled, anything else means the literal
            // terminated early and the input is a larger expression
            if (i == literal.Length - 2 || literal[i + 1] != '\'')
            {
                isSingleLiteral = false;
                break;
            }

            builder.Append('\'');
            i++;
        }

        var result = builder.GetStringAndRelease();
        if (!isSingleLiteral)
            return false;

        value = result;
        return true;
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

    [GeneratedRegex(@"^[+-]?(\d+(\.\d*)?|\.\d+)([eE][+-]?\d+)?$", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex NumericLiteralRegex();
}
