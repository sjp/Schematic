using System;
using System.Text.RegularExpressions;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql;

/// <summary>
/// Builds the default value of a column from the <c>column_default</c> and <c>extra</c> text MySQL
/// reports, classifying it so that consumers do not have to parse the expression themselves. MySQL
/// models a default as a property of the column rather than as a named constraint, so a default
/// built here never carries a constraint name, and it has no sequence objects for one to draw from.
/// </summary>
internal static partial class MySqlDefaultValueParser
{
    private const string DefaultGenerated = "DEFAULT_GENERATED";

    public static Option<IDatabaseDefaultValue> Parse(string? definition, string? extraInformation)
    {
        if (definition.IsNullOrWhiteSpace())
            return Option<IDatabaseDefaultValue>.None;

        return Option<IDatabaseDefaultValue>.Some(new DatabaseDefaultValue(definition, Classify(definition, extraInformation)));
    }

    private static DefaultValueKind Classify(string definition, string? extraInformation)
    {
        // 'extra' marks a default that the server evaluates for each row rather than stores, which
        // is the only way to tell an expression apart from a literal that happens to look like one
        if (extraInformation?.Contains(DefaultGenerated, StringComparison.OrdinalIgnoreCase) == true)
            return DefaultValueKind.Expression;

        var value = definition.Trim();

        if (string.Equals(value, "NULL", StringComparison.OrdinalIgnoreCase))
            return DefaultValueKind.Null;

        // a temporal default predates expression defaults, so servers that do not mark one as
        // generated still report it here
        if (CurrentTimestampRegex().IsMatch(value))
            return DefaultValueKind.Expression;

        // information_schema reports the value of an ordinary default verbatim and unquoted, so
        // whatever is left is the literal itself
        return DefaultValueKind.Literal;
    }

    [GeneratedRegex(@"^(current_timestamp|localtime|localtimestamp|now)(\(\s*\d*\s*\))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 100)]
    private static partial Regex CurrentTimestampRegex();
}
