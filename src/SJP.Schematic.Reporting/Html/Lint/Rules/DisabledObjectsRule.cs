using System;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class DisabledObjectsRule : Schematic.Lint.Rules.DisabledObjectsRule
{
    public DisabledObjectsRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildDisabledForeignKeyMessage(Identifier tableName, Option<Identifier> foreignKeyName)
    {
        var messageKeyName = foreignKeyName.Match(
            static name => " '" + name.LocalName + "'",
            static () => string.Empty
        );

        var messageText = $"The table {tableName.ToVisibleName()} contains a disabled foreign key{messageKeyName}. Consider enabling or removing the foreign key.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildDisabledPrimaryKeyMessage(Identifier tableName, Option<Identifier> primaryKeyName)
    {
        var messageKeyName = primaryKeyName.Match(
               static name => " '" + name.LocalName + "'",
               static () => string.Empty
           );

        var messageText = $"The table {tableName.ToVisibleName()} contains a disabled primary key{messageKeyName}. Consider enabling or removing the primary key.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildDisabledUniqueKeyMessage(Identifier tableName, Option<Identifier> uniqueKeyName)
    {
        var messageKeyName = uniqueKeyName.Match(
            static name => " '" + name.LocalName + "'",
            static () => string.Empty
        );

        var messageText = $"The table {tableName.ToVisibleName()} contains a disabled unique key{messageKeyName}. Consider enabling or removing the unique key.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildDisabledCheckConstraintMessage(Identifier tableName, Option<Identifier> checkName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageCheckName = checkName.Match(
            static name => " '" + name.LocalName + "'",
            static () => string.Empty
        );

        var messageText = $"The table {tableName.ToVisibleName()} contains a disabled check constraint{messageCheckName}. Consider enabling or removing the check constraint.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildDisabledIndexMessage(Identifier tableName, string? indexName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageIndexName = !indexName.IsNullOrWhiteSpace()
            ? " '" + indexName + "'"
            : string.Empty;

        var messageText = $"The table {tableName.ToVisibleName()} contains a disabled index{messageIndexName}. Consider enabling or removing the index.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    protected override IRuleMessage BuildDisabledTriggerMessage(Identifier tableName, string? triggerName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageTriggerName = !triggerName.IsNullOrWhiteSpace()
            ? " '" + triggerName + "'"
            : string.Empty;

        var messageText = $"The table {tableName.ToVisibleName()} contains a disabled trigger{messageTriggerName}. Consider enabling or removing the trigger.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
