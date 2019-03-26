using System;
using System.Data;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal sealed class InvalidViewDefinitionRule : Schematic.Lint.Rules.InvalidViewDefinitionRule
    {
        public InvalidViewDefinitionRule(IDbConnection connection, IDatabaseDialect dialect, RuleLevel level)
            : base(connection, dialect, level)
        {
        }

        protected override IRuleMessage BuildMessage(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var viewUrl = UrlRouter.GetViewUrl(viewName);
            var viewLink = $"<a href=\"{ viewUrl }\">{ HttpUtility.HtmlEncode(viewName.ToVisibleName()) }</a>";
            var messageText = $"The view { viewLink } was unable to be queried. This may indicate an incorrect view definition.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
