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

    private const string NullDefault = "NULL";

    /// <summary>
    /// Builds the default value of a column, if it has one.
    /// </summary>
    /// <param name="definition">The default as reported by <c>column_default</c>.</param>
    /// <param name="extraInformation">The column's <c>extra</c> text, which marks a default the server evaluates per row.</param>
    /// <returns>A default value, or none when the column has no default.</returns>
    /// <remarks>
    /// MariaDB reports a nullable column that has no default -- and one declared <c>DEFAULT NULL</c>,
    /// which means the same thing -- as the unquoted text <c>NULL</c> rather than as SQL <c>NULL</c>,
    /// so without discarding it every nullable column would carry a default. MySQL reports SQL
    /// <c>NULL</c> for both, and unlike MariaDB prints a string literal unquoted, so a MySQL text
    /// column whose default is the word <c>NULL</c> is indistinguishable from having none.
    /// </remarks>
    public static Option<IDatabaseDefaultValue> Parse(string? definition, string? extraInformation)
    {
        if (definition.IsNullOrWhiteSpace()
            || string.Equals(definition.Trim(), NullDefault, StringComparison.OrdinalIgnoreCase))
            return Option<IDatabaseDefaultValue>.None;

        return Option<IDatabaseDefaultValue>.Some(new DatabaseDefaultValue(definition, Classify(definition, extraInformation)));
    }

    private static DefaultValueKind Classify(string definition, string? extraInformation)
    {
        // 'extra' marks a default that the server evaluates for each row rather than stores, which
        // is the only way to tell an expression apart from a literal that happens to look like one
        if (extraInformation?.Contains(DefaultGenerated, StringComparison.OrdinalIgnoreCase) == true)
            return DefaultValueKind.Expression;

        // a temporal default predates expression defaults, so servers that do not mark one as
        // generated still report it here
        if (CurrentTimestampRegex().IsMatch(definition.Trim()))
            return DefaultValueKind.Expression;

        // information_schema reports the value of an ordinary default rather than the SQL that
        // produced it, so whatever is left is the literal itself
        return DefaultValueKind.Literal;
    }

    [GeneratedRegex(@"^(current_timestamp|localtime|localtimestamp|now)(\(\s*\d*\s*\))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 100)]
    private static partial Regex CurrentTimestampRegex();
}
