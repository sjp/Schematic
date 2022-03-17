using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ForeignKeyIndexRule : Schematic.Lint.Rules.ForeignKeyIndexRule
{
    public ForeignKeyIndexRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier tableName, IEnumerable<string> columnNames)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (columnNames == null || columnNames.Empty())
            throw new ArgumentNullException(nameof(columnNames));

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
        var builder = StringBuilderCache.Acquire();
        builder.Append("The table ")
            .Append(tableLink)
            .Append(" has a foreign key");

        foreignKeyName.IfSome(fkName =>
        {
            builder.Append(" <code>")
                .Append(HttpUtility.HtmlEncode(fkName.LocalName))
                .Append("</code>");
        });

        builder.Append(" which is missing an index on the column");

        // plural check
        if (columnNames.Skip(1).Any())
            builder.Append('s');

        var formattedColumnNames = columnNames
            .Select(columnName => "<code>" + HttpUtility.HtmlEncode(columnName) + "</code>");
        builder.Append(' ')
            .AppendJoin(", ", formattedColumnNames);

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
