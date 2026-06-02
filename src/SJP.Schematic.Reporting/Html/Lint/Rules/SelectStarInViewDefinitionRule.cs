using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class SelectStarInViewDefinitionRule : Schematic.Lint.Rules.SelectStarInViewDefinitionRule
{
    public SelectStarInViewDefinitionRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var messageText = $"The view {viewName.ToVisibleName()} selects all columns using a '*' wildcard. This makes the view brittle, as its result set silently changes when the underlying tables change. Consider listing columns explicitly.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
