using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ForeignKeyMissingRule : Schematic.Lint.Rules.ForeignKeyMissingRule
{
    public ForeignKeyMissingRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(string columnName, Identifier tableName, Identifier targetTableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(targetTableName);

        var builder = StringBuilderCache.Acquire();
        builder.Append("The table ")
            .Append(tableName.ToVisibleName())
            .Append(" has a column '")
            .Append(columnName)
            .Append("' implying a relationship to ")
            .Append(targetTableName.ToVisibleName())
            .Append(" which is missing a foreign key constraint.");

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
