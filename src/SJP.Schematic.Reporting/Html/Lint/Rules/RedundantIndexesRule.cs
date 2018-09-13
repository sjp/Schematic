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
    internal sealed class RedundantIndexesRule : Schematic.Lint.Rules.RedundantIndexesRule
    {
        public RedundantIndexesRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(Identifier tableName, string indexName, IEnumerable<string> redundantIndexColumnNames, string otherIndexName, IEnumerable<string> otherIndexColumnNames)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (indexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(indexName));
            if (redundantIndexColumnNames == null || redundantIndexColumnNames.Empty())
                throw new ArgumentNullException(nameof(redundantIndexColumnNames));
            if (otherIndexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(otherIndexName));
            if (otherIndexColumnNames == null || otherIndexColumnNames.Empty())
                throw new ArgumentNullException(nameof(otherIndexColumnNames));

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";

            var columnNames = redundantIndexColumnNames
                .Select(columnName => "<code>" + HttpUtility.HtmlEncode(columnName) + "</code>")
                .Join(", ");
            var otherColumnNames = otherIndexColumnNames
                .Select(columnName => "<code>" + HttpUtility.HtmlEncode(columnName) + "</code>")
                .Join(", ");

            var builder = StringBuilderCache.Acquire();
            builder.Append("The table ")
                .Append(tableLink)
                .Append(" has an index <code>")
                .Append(HttpUtility.HtmlEncode(indexName))
                .Append("</code> which may be redundant, as its column set (")
                .Append(columnNames)
                .Append(") is the prefix of another index <code>")
                .Append(HttpUtility.HtmlEncode(otherIndexName))
                .Append("</code> (")
                .Append(otherColumnNames)
                .Append(").");

            var messageText = builder.GetStringAndRelease();
            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
