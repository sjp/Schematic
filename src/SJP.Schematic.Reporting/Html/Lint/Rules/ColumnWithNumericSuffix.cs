using System;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal class ColumnWithNumericSuffix : Schematic.Lint.Rules.ColumnWithNumericSuffix
    {
        public ColumnWithNumericSuffix(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(Identifier tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var tableUrl = UrlRouter.GetTableUrl(tableName);
            var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table { tableLink } has a column <code>{ HttpUtility.HtmlEncode(columnName) }</code> with a numeric suffix, indicating denormalization.";

            return new RuleMessage(RuleId, RuleTitle, Level, messageText);
        }
    }
}
