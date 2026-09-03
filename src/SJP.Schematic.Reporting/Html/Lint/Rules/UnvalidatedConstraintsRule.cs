using System;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class UnvalidatedConstraintsRule : Schematic.Lint.Rules.UnvalidatedConstraintsRule
{
    public UnvalidatedConstraintsRule(RuleLevel? level = null)
        : base(level)
    {
    }

    protected override IRuleMessage BuildUnvalidatedForeignKeyMessage(Identifier tableName, Option<Identifier> foreignKeyName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageKeyName = GetConstraintNameSuffix(foreignKeyName);
        var messageText = $"The table {tableName.ToVisibleName()} contains an unvalidated foreign key{messageKeyName}. The database will not rely upon it when planning queries. Consider validating the existing rows.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    protected override IRuleMessage BuildUnvalidatedPrimaryKeyMessage(Identifier tableName, Option<Identifier> primaryKeyName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageKeyName = GetConstraintNameSuffix(primaryKeyName);
        var messageText = $"The table {tableName.ToVisibleName()} contains an unvalidated primary key{messageKeyName}. The database will not rely upon it when planning queries. Consider validating the existing rows.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    protected override IRuleMessage BuildUnvalidatedUniqueKeyMessage(Identifier tableName, Option<Identifier> uniqueKeyName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageKeyName = GetConstraintNameSuffix(uniqueKeyName);
        var messageText = $"The table {tableName.ToVisibleName()} contains an unvalidated unique key{messageKeyName}. The database will not rely upon it when planning queries. Consider validating the existing rows.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    protected override IRuleMessage BuildUnvalidatedCheckConstraintMessage(Identifier tableName, Option<Identifier> checkName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageCheckName = GetConstraintNameSuffix(checkName);
        var messageText = $"The table {tableName.ToVisibleName()} contains an unvalidated check constraint{messageCheckName}. Existing rows are not known to satisfy it. Consider validating the existing rows.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }
}
