using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class InconsistentColumnNamingConventionRule : Schematic.Lint.Rules.InconsistentColumnNamingConventionRule
{
    public InconsistentColumnNamingConventionRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, Identifier columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(columnName);

        var messageText = $"The column '{columnName.LocalName}' in the table {tableName.ToVisibleName()} does not follow the dominant naming convention used elsewhere in the schema. Consider using a consistent convention for all column names.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
