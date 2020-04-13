using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal sealed class UniqueIndexWithNullableColumnsRule : Schematic.Lint.Rules.UniqueIndexWithNullableColumnsRule
    {
        public UniqueIndexWithNullableColumnsRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(Identifier tableName, string? indexName, IEnumerable<string> columnNames)
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
                .Append(" has a unique index");

            if (!indexName.IsNullOrWhiteSpace())
            {
                builder.Append(" <code>")
                    .Append(HttpUtility.HtmlEncode(indexName))
                    .Append("</code>");
            }

            var pluralText = columnNames.Skip(1).Any()
                ? " which contains nullable columns: "
                : " which contains a nullable column: ";
            builder.Append(pluralText);

            var formattedColumnNames = columnNames
                .Select(columnName => "<code>" + HttpUtility.HtmlEncode(columnName) + "</code>");
            builder.AppendJoin(", ", formattedColumnNames);

            var messageText = builder.GetStringAndRelease();
            return new RuleMessage(RuleId, RuleTitle, Level, messageText);
        }
    }
}
