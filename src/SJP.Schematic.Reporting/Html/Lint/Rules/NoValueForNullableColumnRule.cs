using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class NoValueForNullableColumnRule : Schematic.Lint.Rules.NoValueForNullableColumnRule
{
    public NoValueForNullableColumnRule(ISchematicConnection connection, RuleLevel level)
        : base(connection, level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var messageText = $"The table {tableName.ToVisibleName()} has a nullable column '{columnName}' whose values are always NULL. Consider removing the column.";

        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
