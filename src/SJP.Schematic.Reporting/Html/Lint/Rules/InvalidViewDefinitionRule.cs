﻿using System;
using System.Data;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal class InvalidViewDefinitionRule : Schematic.Lint.Rules.InvalidViewDefinitionRule
    {
        public InvalidViewDefinitionRule(IDbConnection connection, RuleLevel level)
            : base(connection, level)
        {
        }

        protected override IRuleMessage BuildMessage(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var viewLink = $"<a href=\"views/{ viewName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(viewName.ToVisibleName()) }</a>";
            var messageText = $"The view { viewLink } was unable to be queried. This may indicate an incorrect view definition.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
