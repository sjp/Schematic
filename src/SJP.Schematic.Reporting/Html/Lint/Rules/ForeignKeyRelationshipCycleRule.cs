using System;
using System.Web;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules
{
    internal class ForeignKeyRelationshipCycleRule : Schematic.Lint.Rules.ForeignKeyRelationshipCycleRule
    {
        public ForeignKeyRelationshipCycleRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(string exceptionMessage)
        {
            if (exceptionMessage.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(exceptionMessage));

            return new RuleMessage(RuleTitle, Level, HttpUtility.HtmlEncode(exceptionMessage));
        }
    }
}
