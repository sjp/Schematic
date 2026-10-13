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
    /// <param name="isMariaDb">Whether the server is MariaDB, which reports a default as SQL text rather than as its value.</param>
    /// <returns>A default value, or none when the column has no default.</returns>
    /// <remarks>
    /// <para>
    /// MariaDB reports a nullable column that has no default -- and one declared <c>DEFAULT NULL</c>,
    /// which means the same thing -- as the unquoted text <c>NULL</c> rather than as SQL <c>NULL</c>,
    /// so without discarding it every nullable column would carry a default. MySQL reports SQL
    /// <c>NULL</c> for both, and unlike MariaDB prints a string literal unquoted, so a MySQL text
    /// column whose default is the word <c>NULL</c> is indistinguishable from having none.
    /// </para>
    /// <para>
    /// MySQL also reports an empty or whitespace string default as that string, which is not a usable
    /// definition on its own, so it is returned quoted, the way MariaDB reports it.
    /// </para>
    /// </remarks>
    public static Option<IDatabaseDefaultValue> Parse(string? definition, string? extraInformation, bool isMariaDb)
    {
        if (definition == null)
            return Option<IDatabaseDefaultValue>.None;

        if (definition.IsNullOrWhiteSpace())
        {
            // MariaDB quotes every string default, so there is nothing to recover from blank text
            return isMariaDb
                ? Option<IDatabaseDefaultValue>.None
                : Option<IDatabaseDefaultValue>.Some(new DatabaseDefaultValue("'" + definition + "'", DefaultValueKind.Literal));
        }

        if (string.Equals(definition.Trim(), NullDefault, StringComparison.OrdinalIgnoreCase))
            return Option<IDatabaseDefaultValue>.None;

        var kind = isMariaDb
            ? ClassifyMariaDb(definition)
            : ClassifyMySql(definition, extraInformation);
        return Option<IDatabaseDefaultValue>.Some(new DatabaseDefaultValue(definition, kind));
    }

    private static DefaultValueKind ClassifyMySql(string definition, string? extraInformation)
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

    private static DefaultValueKind ClassifyMariaDb(string definition)
    {
        // MariaDB never marks a default as generated, but it reports a default as SQL: strings and
        // temporal values quoted, numbers and bit or hex values as literals, and anything else as
        // the expression that was declared (with constant expressions already folded to a literal)
        var trimmed = definition.Trim();
        return trimmed.StartsWith('\'') || NumericLiteralRegex().IsMatch(trimmed) || BinaryLiteralRegex().IsMatch(trimmed)
            ? DefaultValueKind.Literal
            : DefaultValueKind.Expression;
    }

    [GeneratedRegex(@"^[+-]?([0-9]+\.?[0-9]*|\.[0-9]+)([eE][+-]?[0-9]+)?$", RegexOptions.Compiled | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex NumericLiteralRegex();

    [GeneratedRegex(@"^([bx]'[0-9a-f]*'|0x[0-9a-f]+|0b[01]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex BinaryLiteralRegex();

    [GeneratedRegex(@"^(current_timestamp|localtime|localtimestamp|now)(\(\s*\d*\s*\))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 100)]
    private static partial Regex CurrentTimestampRegex();
}
