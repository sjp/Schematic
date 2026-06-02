using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class InvalidViewDefinitionRule : Schematic.Lint.Rules.InvalidViewDefinitionRule
{
    public InvalidViewDefinitionRule(ISchematicConnection connection, RuleLevel level)
        : base(connection, level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var messageText = $"The view {viewName.ToVisibleName()} was unable to be queried. This may indicate an incorrect view definition.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
