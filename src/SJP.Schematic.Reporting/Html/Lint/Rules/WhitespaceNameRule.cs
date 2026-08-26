using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class WhitespaceNameRule : Schematic.Lint.Rules.WhitespaceNameRule
{
    public WhitespaceNameRule(RuleLevel? level = null)
        : base(level)
    {
    }

    protected override IRuleMessage BuildTableMessage(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageText = $"The table {tableName.ToVisibleName()} contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    protected override IRuleMessage BuildTableColumnMessage(Identifier tableName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var messageText = $"The table {tableName.ToVisibleName()} contains a column '{columnName}' which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    protected override IRuleMessage BuildTableIndexMessage(Identifier tableName, string indexName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        var messageText = $"The table {tableName.ToVisibleName()} contains an index '{indexName}' which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    protected override IRuleMessage BuildTablePrimaryKeyMessage(Identifier tableName, string primaryKeyName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(primaryKeyName);

        var messageText = $"The table {tableName.ToVisibleName()} contains a primary key '{primaryKeyName}' which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    protected override IRuleMessage BuildTableUniqueKeyMessage(Identifier tableName, string uniqueKeyName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(uniqueKeyName);

        var messageText = $"The table {tableName.ToVisibleName()} contains a unique key '{uniqueKeyName}' which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    protected override IRuleMessage BuildTableForeignKeyMessage(Identifier tableName, string foreignKeyName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(foreignKeyName);

        var messageText = $"The table {tableName.ToVisibleName()} contains a foreign key '{foreignKeyName}' which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    protected override IRuleMessage BuildTableCheckConstraintMessage(Identifier tableName, string checkName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(checkName);

        var messageText = $"The table {tableName.ToVisibleName()} contains a check constraint '{checkName}' which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    protected override IRuleMessage BuildTableTriggerMessage(Identifier tableName, string triggerName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(triggerName);

        var messageText = $"The table {tableName.ToVisibleName()} contains a trigger '{triggerName}' which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    protected override IRuleMessage BuildViewMessage(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var messageText = $"The view {viewName.ToVisibleName()} contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, viewName);
    }

    protected override IRuleMessage BuildViewColumnMessage(Identifier viewName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(viewName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var messageText = $"The view {viewName.ToVisibleName()} contains a column '{columnName}' which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, viewName);
    }

    protected override IRuleMessage BuildSequenceMessage(Identifier sequenceName)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        var messageText = $"The sequence {sequenceName.ToVisibleName()} contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, sequenceName);
    }

    protected override IRuleMessage BuildSynonymMessage(Identifier synonymName)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        var messageText = $"The synonym {synonymName.ToVisibleName()} contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, synonymName);
    }

    protected override IRuleMessage BuildRoutineMessage(Identifier routineName)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var messageText = $"The routine {routineName.ToVisibleName()} contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, routineName);
    }
}
