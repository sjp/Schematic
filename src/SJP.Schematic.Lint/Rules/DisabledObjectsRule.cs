using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class DisabledObjectsRule : Rule
    {
        public DisabledObjectsRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public override IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return database.Tables.SelectMany(AnalyseTable).ToList();
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var result = new List<IRuleMessage>();

            var disabledForeignKeys = table.ParentKeys
                .Select(fk => fk.ChildKey)
                .Where(fk => !fk.IsEnabled);
            foreach (var foreignKey in disabledForeignKeys)
            {
                var ruleMessage = BuildDisabledForeignKeyMessage(table.Name, foreignKey.Name?.LocalName);
                result.Add(ruleMessage);
            }

            var primaryKey = table.PrimaryKey;
            if (primaryKey != null && !primaryKey.IsEnabled)
            {
                var ruleMessage = BuildDisabledPrimaryKeyMessage(table.Name, primaryKey.Name?.LocalName);
                result.Add(ruleMessage);
            }

            var disabledUniqueKeys = table.UniqueKeys.Where(uk => !uk.IsEnabled);
            foreach (var uniqueKey in disabledUniqueKeys)
            {
                var ruleMessage = BuildDisabledUniqueKeyMessage(table.Name, uniqueKey.Name?.LocalName);
                result.Add(ruleMessage);
            }

            var disabledChecks = table.Checks.Where(ck => !ck.IsEnabled);
            foreach (var check in disabledChecks)
            {
                var ruleMessage = BuildDisabledCheckConstraintMessage(table.Name, check.Name?.LocalName);
                result.Add(ruleMessage);
            }

            var disabledIndexes = table.Indexes.Where(ix => !ix.IsEnabled);
            foreach (var index in disabledIndexes)
            {
                var ruleMessage = BuildDisabledIndexMessage(table.Name, index.Name?.LocalName);
                result.Add(ruleMessage);
            }

            var disabledTriggers = table.Triggers.Where(uk => !uk.IsEnabled);
            foreach (var trigger in disabledTriggers)
            {
                var ruleMessage = BuildDisabledTriggerMessage(table.Name, trigger.Name?.LocalName);
                result.Add(ruleMessage);
            }

            return result;
        }

        protected virtual IRuleMessage BuildDisabledForeignKeyMessage(Identifier tableName, string foreignKeyName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageKeyName = !foreignKeyName.IsNullOrWhiteSpace()
                ? " '" + foreignKeyName + "'"
                : string.Empty;

            var messageText = $"The table '{ tableName }' contains a disabled foreign key{ messageKeyName }. Consider enabling or removing the foreign key.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildDisabledPrimaryKeyMessage(Identifier tableName, string primaryKeyName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageKeyName = !primaryKeyName.IsNullOrWhiteSpace()
                ? " '" + primaryKeyName + "'"
                : string.Empty;

            var messageText = $"The table '{ tableName }' contains a disabled primary key{ messageKeyName }. Consider enabling or removing the primary key.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildDisabledUniqueKeyMessage(Identifier tableName, string uniqueKeyName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageKeyName = !uniqueKeyName.IsNullOrWhiteSpace()
                ? " '" + uniqueKeyName + "'"
                : string.Empty;

            var messageText = $"The table '{ tableName }' contains a disabled unique key{ messageKeyName }. Consider enabling or removing the unique key.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildDisabledCheckConstraintMessage(Identifier tableName, string checkName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageCheckName = !checkName.IsNullOrWhiteSpace()
                ? " '" + checkName + "'"
                : string.Empty;

            var messageText = $"The table '{ tableName }' contains a disabled check constraint{ messageCheckName }. Consider enabling or removing the check constraint.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildDisabledIndexMessage(Identifier tableName, string indexName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageIndexName = !indexName.IsNullOrWhiteSpace()
                ? " '" + indexName + "'"
                : string.Empty;

            var messageText = $"The table '{ tableName }' contains a disabled index{ messageIndexName }. Consider enabling or removing the index.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildDisabledTriggerMessage(Identifier tableName, string triggerName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageTriggerName = !triggerName.IsNullOrWhiteSpace()
                ? " '" + triggerName + "'"
                : string.Empty;

            var messageText = $"The table '{ tableName }' contains a disabled trigger{ messageTriggerName }. Consider enabling or removing the trigger.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Disabled constraint, index or triggers present on a table.";
    }
}
