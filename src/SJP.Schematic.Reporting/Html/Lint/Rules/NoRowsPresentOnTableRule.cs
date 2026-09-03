using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class NoRowsPresentOnTableRule : Schematic.Lint.Rules.NoRowsPresentOnTableRule
{
    public NoRowsPresentOnTableRule(ISchematicConnection connection, RuleLevel? level = null, ITableStatisticsProvider? tableStatistics = null)
        : base(connection, level, tableStatistics)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageText = $"The table {tableName.ToVisibleName()} contains no rows. Consider removing it if it is unused.";

        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }
}
