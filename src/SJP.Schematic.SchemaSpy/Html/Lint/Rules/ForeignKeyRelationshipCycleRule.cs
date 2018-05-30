using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.SchemaSpy.Html.Lint.Rules
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
