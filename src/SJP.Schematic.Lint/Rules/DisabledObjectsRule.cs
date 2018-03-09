using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

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
                var foreignKeyName = foreignKey.Name?.LocalName != null ? $" '{ foreignKey.Name.LocalName }'" : string.Empty;
                var messageText = $"The table '{ table.Name }' contains a disabled foreign key{ foreignKeyName }. Consider enabling or removing the foreign key.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            var primaryKey = table.PrimaryKey;
            if (primaryKey != null && !primaryKey.IsEnabled)
            {
                var uniqueKeyName = primaryKey.Name?.LocalName != null ? $" '{ primaryKey.Name.LocalName }'" : string.Empty;
                var messageText = $"The table '{ table.Name }' contains a disabled primary key{ uniqueKeyName }. Consider enabling or removing the primary key.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            var disabledUniqueKeys = table.UniqueKeys.Where(uk => !uk.IsEnabled);
            foreach (var uniqueKey in disabledUniqueKeys)
            {
                var uniqueKeyName = uniqueKey.Name?.LocalName != null ? $" '{ uniqueKey.Name.LocalName }'" : string.Empty;
                var messageText = $"The table '{ table.Name }' contains a disabled unique key{ uniqueKeyName }. Consider enabling or removing the unique key.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            var disabledChecks = table.Checks.Where(ck => !ck.IsEnabled);
            foreach (var checks in disabledChecks)
            {
                var checkName = checks.Name?.LocalName != null ? $" '{ checks.Name.LocalName }'" : string.Empty;
                var messageText = $"The table '{ table.Name }' contains a disabled check constraint{ checkName }. Consider enabling or removing the check constraint.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            var disabledIndexes = table.Indexes.Where(ix => !ix.IsEnabled);
            foreach (var index in disabledIndexes)
            {
                var indexName = index.Name?.LocalName != null ? $" '{ index.Name.LocalName }'" : string.Empty;
                var messageText = $"The table '{ table.Name }' contains a disabled index{ indexName }. Consider enabling or removing the index.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            var disabledTriggers = table.Triggers.Where(uk => !uk.IsEnabled);
            foreach (var trigger in disabledTriggers)
            {
                var triggerName = trigger.Name?.LocalName != null ? $" '{ trigger.Name.LocalName }'" : string.Empty;
                var messageText = $"The table '{ table.Name }' contains a disabled trigger{ triggerName }. Consider enabling or removing the trigger.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            return result;
        }

        private const string RuleTitle = "Disabled constraint, index or triggers present on a table.";
    }
}
