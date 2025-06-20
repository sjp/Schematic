using System;
using System.Linq;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class RedundantIndexesRule : Schematic.Lint.Rules.RedundantIndexesRule
{
    public RedundantIndexesRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, IDatabaseIndex redundantIndex, IDatabaseIndex otherIndex)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(redundantIndex);
        ArgumentNullException.ThrowIfNull(otherIndex);

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{tableUrl}\">{HttpUtility.HtmlEncode(tableName.ToVisibleName())}</a>";

        var redundantIndexColumnNames = redundantIndex.Columns
            .SelectMany(c => c.DependentColumns)
            .Select(c => c.Name.LocalName)
            .Select(EncodeColumnName)
            .ToList();
        var redundantIncludedColumnNames = redundantIndex.IncludedColumns
            .Select(c => c.Name.LocalName)
            .Select(EncodeColumnName)
            .ToList();
        var otherIndexColumnNames = otherIndex.Columns
            .SelectMany(c => c.DependentColumns)
            .Select(c => c.Name.LocalName)
            .Select(EncodeColumnName)
            .ToList();
        var otherIncludedColumnNames = otherIndex.IncludedColumns
            .Select(c => c.Name.LocalName)
            .Select(EncodeColumnName)
            .ToList();

        var builder = StringBuilderCache.Acquire();
        builder.Append("The table ")
            .Append(tableLink)
            .Append(" has an index <code>")
            .Append(HttpUtility.HtmlEncode(redundantIndex.Name.LocalName))
            .Append("</code> which may be redundant, as its column set (")
            .AppendJoin(", ", redundantIndexColumnNames)
            .Append(')');

        if (redundantIndex.IncludedColumns.Count > 0)
        {
            builder.Append(" INCLUDE (")
                .AppendJoin(", ", redundantIncludedColumnNames)
                .Append(')');
        }

        builder
            .Append(" is the prefix or subset of another index <code>")
            .Append(HttpUtility.HtmlEncode(otherIndex.Name.LocalName))
            .Append("</code> (")
            .AppendJoin(", ", otherIndexColumnNames)
            .Append(')');

        if (otherIndex.IncludedColumns.Count > 0)
        {
            builder.Append(" INCLUDE (")
                .AppendJoin(", ", otherIncludedColumnNames)
                .Append(')');
        }

        builder.Append('.');

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    private static string EncodeColumnName(string columnName) => "<code>" + HttpUtility.HtmlEncode(columnName) + "</code>";
}