using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ForeignKeyRelationshipCycleRule : Schematic.Lint.Rules.ForeignKeyRelationshipCycleRule
{
    public ForeignKeyRelationshipCycleRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(IReadOnlyCollection<Identifier> cyclePath)
    {
        if (cyclePath == null)
            throw new ArgumentNullException(nameof(cyclePath));

        var tableNames = cyclePath
            .Select(static tableName =>
            {
                var tableUrl = UrlRouter.GetTableUrl(tableName);
                return $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            })
            .Join(" &rarr; ");
        var message = "Cycle found for the following path: " + tableNames;

        return new RuleMessage(RuleId, RuleTitle, Level, message);
    }
}