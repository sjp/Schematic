using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ForeignKeyMissingRule : Schematic.Lint.Rules.ForeignKeyMissingRule
{
    public ForeignKeyMissingRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(string columnName, Identifier tableName, Identifier targetTableName)
    {
        if (columnName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(columnName));
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (targetTableName == null)
            throw new ArgumentNullException(nameof(targetTableName));

        var builder = StringBuilderCache.Acquire();

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
        var targetTableUrl = UrlRouter.GetTableUrl(targetTableName);
        var targetTableLink = $"<a href=\"{ targetTableUrl }\">{ HttpUtility.HtmlEncode(targetTableName.ToVisibleName()) }</a>";

        builder.Append("The table ")
            .Append(tableLink)
            .Append(" has a column <code>")
            .Append(columnName)
            .Append("</code> implying a relationship to ")
            .Append(targetTableLink)
            .Append(" which is missing a foreign key constraint.");

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
