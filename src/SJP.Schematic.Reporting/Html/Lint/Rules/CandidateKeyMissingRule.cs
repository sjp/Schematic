using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class CandidateKeyMissingRule : Schematic.Lint.Rules.CandidateKeyMissingRule
{
    public CandidateKeyMissingRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageText = $"The table {tableName.ToVisibleName()} has no candidate (primary or unique) keys. Consider adding one to ensure records are unique.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
