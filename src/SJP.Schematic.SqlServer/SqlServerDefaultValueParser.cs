using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// Builds the default value of a column from the rows SQL Server returns for
/// <c>sys.default_constraints</c>, classifying the constraint's definition so that consumers do not
/// have to parse T-SQL themselves.
/// </summary>
internal static partial class SqlServerDefaultValueParser
{
    public static Option<IDatabaseDefaultValue> Parse(string? definition, string? constraintName)
    {
        if (definition.IsNullOrWhiteSpace())
            return Option<IDatabaseDefaultValue>.None;

        var name = !constraintName.IsNullOrWhiteSpace()
            ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(constraintName))
            : Option<Identifier>.None;

        var (kind, sequenceName) = Classify(definition);

        return Option<IDatabaseDefaultValue>.Some(new DatabaseDefaultValue(definition, kind, name, sequenceName));
    }

    private static (DefaultValueKind Kind, Option<Identifier> SequenceName) Classify(string definition)
    {
        // sys.default_constraints wraps a definition in parentheses, and adds a further pair around
        // a scalar, so 'default 0' is reported as '((0))'.
        var value = RemoveEnclosingParentheses(definition.Trim());

        if (string.Equals(value, "NULL", StringComparison.OrdinalIgnoreCase))
            return (DefaultValueKind.Null, Option<Identifier>.None);

        if (NumericLiteralRegex().IsMatch(value) || IsStringLiteral(value))
            return (DefaultValueKind.Literal, Option<Identifier>.None);

        var sequenceMatch = NextValueForRegex().Match(value);
        if (sequenceMatch.Success)
            return (DefaultValueKind.SequenceNextValue, ParseQualifiedName(sequenceMatch.Groups[1].Value));

        return (DefaultValueKind.Expression, Option<Identifier>.None);
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
                    // the first pair closed before the end of the input, so the whole
                    // expression is not enclosed by it
                    if (depth == 0 && i < input.Length - 1)
                        return false;
                    break;
            }
        }

        return depth == 0;
    }

    private static bool IsStringLiteral(string input)
    {
        // a national character literal (e.g. N'test') carries the same text
        var literal = input.Length > 2 && (input[0] == 'N' || input[0] == 'n') && input[1] == '\''
            ? input[1..]
            : input;

        if (literal.Length < 2 || literal[0] != '\'' || literal[^1] != '\'')
            return false;

        for (var i = 1; i < literal.Length - 1; i++)
        {
            if (literal[i] != '\'')
                continue;

            // an embedded quote must be doubled, anything else means the literal terminated
            // early and the input is a larger expression
            if (i == literal.Length - 2 || literal[i + 1] != '\'')
                return false;

            i++;
        }

        return true;
    }

    // A sequence in a 'next value for' clause is written as a schema-qualified name whose parts may
    // each be bracket-quoted, e.g. [dbo].[order id seq].
    private static Option<Identifier> ParseQualifiedName(string input)
    {
        var parts = new List<string>();
        var start = 0;
        var quoted = false;

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c == '[')
                quoted = true;
            else if (c == ']')
                quoted = false;
            else if (c == '.' && !quoted)
            {
                parts.Add(Unquote(input[start..i]));
                start = i + 1;
            }
        }

        parts.Add(Unquote(input[start..]));
        if (parts.Exists(static part => part.Length == 0))
            return Option<Identifier>.None;

        return parts.Count switch
        {
            1 => Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(parts[0])),
            2 => Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(parts[0], parts[1])),
            3 => Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(parts[0], parts[1], parts[2])),
            _ => Option<Identifier>.None
        };
    }

    private static string Unquote(string input)
    {
        var trimmed = input.Trim();

        return trimmed.Length > 1 && trimmed[0] == '[' && trimmed[^1] == ']'
            ? trimmed[1..^1].Replace("]]", "]", StringComparison.Ordinal)
            : trimmed;
    }

    [GeneratedRegex(@"^[+-]?(\d+(\.\d*)?|\.\d+)([eE][+-]?\d+)?$", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex NumericLiteralRegex();

    [GeneratedRegex(@"^next\s+value\s+for\s+(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline, matchTimeoutMilliseconds: 100)]
    private static partial Regex NextValueForRegex();
}
