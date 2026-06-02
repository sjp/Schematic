using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class TooManyIndexColumnsRule : Schematic.Lint.Rules.TooManyIndexColumnsRule
{
    public TooManyIndexColumnsRule(RuleLevel level, uint columnLimit = 5)
        : base(level, columnLimit)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, Identifier indexName, int columnCount)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(indexName);

        var messageText = $"The table {tableName.ToVisibleName()} has an index '{indexName.LocalName}' with {columnCount.ToString()} columns, which exceeds the configured limit of {ColumnLimit.ToString()}. Consider whether such a wide index is necessary.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
