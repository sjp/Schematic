using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle;

/// <summary>
/// Builds the default value of a column from the <c>DATA_DEFAULT</c> text Oracle reports,
/// classifying it so that consumers do not have to parse the expression themselves. Oracle models a
/// default as a property of the column rather than as a named constraint, so a default built here
/// never carries a constraint name.
/// </summary>
internal static partial class OracleDefaultValueParser
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
        // DATA_DEFAULT is a LONG holding the source text of the clause, which usually keeps the
        // trailing whitespace that followed it in the DDL
        var value = definition.Trim();

        var sequenceName = TryGetSequenceName(value);
        if (sequenceName.IsSome)
            return (DefaultValueKind.SequenceNextValue, sequenceName);

        if (string.Equals(value, "NULL", StringComparison.OrdinalIgnoreCase))
            return (DefaultValueKind.Null, Option<Identifier>.None);

        if (NumericLiteralRegex().IsMatch(value) || IsStringLiteral(value))
            return (DefaultValueKind.Literal, Option<Identifier>.None);

        return (DefaultValueKind.Expression, Option<Identifier>.None);
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

    // A sequence default is a pseudocolumn reference, e.g. "HR"."ORDER_SEQ"."NEXTVAL" for an
    // identity column, or order_seq.nextval as a user would usually write it.
    private static Option<Identifier> TryGetSequenceName(string input)
    {
        var parts = SplitQualifiedName(input);
        if (parts == null || parts.Count < 2)
            return Option<Identifier>.None;

        if (!string.Equals(parts[^1], "NEXTVAL", StringComparison.Ordinal))
            return Option<Identifier>.None;

        parts.RemoveAt(parts.Count - 1);

        return parts.Count switch
        {
            1 => Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(parts[0])),
            2 => Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(parts[0], parts[1])),
            3 => Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(parts[0], parts[1], parts[2])),
            _ => Option<Identifier>.None
        };
    }

    // Returns null when the input is not a plain dotted name, i.e. it is a larger expression.
    private static List<string>? SplitQualifiedName(string input)
    {
        var parts = new List<string>();
        var start = 0;
        var quoted = false;

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c == '"')
            {
                quoted = !quoted;
            }
            else if (c == '.' && !quoted)
            {
                var part = NormalizePart(input[start..i]);
                if (part == null)
                    return null;

                parts.Add(part);
                start = i + 1;
            }
        }

        if (quoted)
            return null;

        var lastPart = NormalizePart(input[start..]);
        if (lastPart == null)
            return null;

        parts.Add(lastPart);
        return parts;
    }

    private static string? NormalizePart(string input)
    {
        var trimmed = input.Trim();
        if (trimmed.Length == 0)
            return null;

        // a quoted name is case sensitive and kept as written; an unquoted one is folded to upper
        // case, which is how the rest of the catalog reports it
        if (trimmed[0] == '"')
        {
            return trimmed.Length > 1 && trimmed[^1] == '"'
                ? trimmed[1..^1]
                : null;
        }

        return UnquotedNameRegex().IsMatch(trimmed)
            ? trimmed.ToUpper(CultureInfo.InvariantCulture)
            : null;
    }

    [GeneratedRegex(@"^[+-]?(\d+(\.\d*)?|\.\d+)([eE][+-]?\d+)?$", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex NumericLiteralRegex();

    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_$#]*$", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex UnquotedNameRegex();
}
