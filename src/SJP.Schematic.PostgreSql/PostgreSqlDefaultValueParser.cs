using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// Builds the default value of a column from the <c>column_default</c> text PostgreSQL reports,
/// classifying it so that consumers do not have to parse the expression themselves. PostgreSQL
/// models a default as a property of the column rather than as a named constraint, so a default
/// built here never carries a constraint name.
/// </summary>
internal static partial class PostgreSqlDefaultValueParser
{
    public static Option<IDatabaseDefaultValue> Parse(string? definition)
    {
        if (definition.IsNullOrWhiteSpace())
            return Option<IDatabaseDefaultValue>.None;

        var (kind, sequenceName) = Classify(definition);

        return Option<IDatabaseDefaultValue>.Some(new DatabaseDefaultValue(definition, kind, Option<Identifier>.None, sequenceName));
    }

    private static (DefaultValueKind Kind, Option<Identifier> SequenceName) Classify(string definition)
    {
        var value = definition.Trim();

        // a serial column is an ordinary column defaulting to the next value of the sequence that
        // was created to own it
        var sequenceMatch = NextValueRegex().Match(value);
        if (sequenceMatch.Success)
            return (DefaultValueKind.SequenceNextValue, ParseSequenceName(sequenceMatch.Groups[1].Value));

        // PostgreSQL stores a default in its parsed form, so a literal is written back out with an
        // explicit cast to the column's type, e.g. 'unassigned'::character varying
        value = RemoveTrailingCasts(value);

        if (string.Equals(value, "NULL", StringComparison.OrdinalIgnoreCase))
            return (DefaultValueKind.Null, Option<Identifier>.None);

        if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
        {
            return (DefaultValueKind.Literal, Option<Identifier>.None);
        }

        if (NumericLiteralRegex().IsMatch(value) || IsStringLiteral(value))
            return (DefaultValueKind.Literal, Option<Identifier>.None);

        return (DefaultValueKind.Expression, Option<Identifier>.None);
    }

    private static string RemoveTrailingCasts(string input)
    {
        var result = input;

        while (true)
        {
            var castMatch = TrailingCastRegex().Match(result);
            if (!castMatch.Success)
                return result;

            var candidate = castMatch.Groups[1].Value.TrimEnd();
            // '::' inside a larger expression is not a cast of the whole value, e.g. a::int + b::int
            if (!IsSelfContained(candidate))
                return result;

            result = candidate;
        }
    }

    private static bool IsSelfContained(string input)
    {
        var depth = 0;
        var inStringLiteral = false;

        foreach (var c in input)
        {
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
                    break;
            }
        }

        return depth == 0 && !inStringLiteral;
    }

    private static bool IsStringLiteral(string input)
    {
        if (input.Length < 2 || input[0] != '\'' || input[^1] != '\'')
            return false;

        for (var i = 1; i < input.Length - 1; i++)
        {
            if (input[i] != '\'')
                continue;

            // an embedded quote must be doubled, anything else means the literal terminated
            // early and the input is a larger expression
            if (i == input.Length - 2 || input[i + 1] != '\'')
                return false;

            i++;
        }

        return true;
    }

    // nextval() names its sequence with a regclass literal, which is a possibly schema-qualified
    // name whose parts are double-quoted only where they need to be.
    private static Option<Identifier> ParseSequenceName(string literal)
    {
        var unescaped = literal.Replace("''", "'", StringComparison.Ordinal);

        var parts = new List<string>();
        var start = 0;
        var quoted = false;

        for (var i = 0; i < unescaped.Length; i++)
        {
            var c = unescaped[i];
            if (c == '"')
                quoted = !quoted;
            else if (c == '.' && !quoted)
            {
                parts.Add(Unquote(unescaped[start..i]));
                start = i + 1;
            }
        }

        parts.Add(Unquote(unescaped[start..]));
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

        return trimmed.Length > 1 && trimmed[0] == '"' && trimmed[^1] == '"'
            ? trimmed[1..^1].Replace("\"\"", "\"", StringComparison.Ordinal)
            : trimmed;
    }

    [GeneratedRegex(@"^[+-]?(\d+(\.\d*)?|\.\d+)([eE][+-]?\d+)?$", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex NumericLiteralRegex();

    [GeneratedRegex(@"^nextval\(\s*'(.*)'(::regclass)?\s*\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline, matchTimeoutMilliseconds: 100)]
    private static partial Regex NextValueRegex();

    [GeneratedRegex(@"^(.*)::\s*[A-Za-z_][A-Za-z0-9_ ]*(\(\s*\d+\s*(,\s*\d+\s*)?\))?(\[\])*$", RegexOptions.Compiled | RegexOptions.Singleline, matchTimeoutMilliseconds: 100)]
    private static partial Regex TrailingCastRegex();
}
