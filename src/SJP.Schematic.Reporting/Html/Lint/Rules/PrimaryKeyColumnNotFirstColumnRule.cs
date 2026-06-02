using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class PrimaryKeyColumnNotFirstColumnRule : Schematic.Lint.Rules.PrimaryKeyColumnNotFirstColumnRule
{
    public PrimaryKeyColumnNotFirstColumnRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageText = $"The table {tableName.ToVisibleName()} has a primary key whose column is not the first column in the table.";

        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
