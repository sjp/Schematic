using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class DisabledObjectsRule : Rule, ITableRule
    {
        public DisabledObjectsRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public IEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return tables.SelectMany(AnalyseTable).ToList();
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            var messages = AnalyseTables(tables);
            return Task.FromResult(messages);
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
                var ruleMessage = BuildDisabledForeignKeyMessage(table.Name, foreignKey.Name);
                result.Add(ruleMessage);
            }

            var primaryKey = table.PrimaryKey;
            primaryKey
                .Where(pk => !pk.IsEnabled)
                .Map(pk => BuildDisabledPrimaryKeyMessage(table.Name, pk.Name))
                .IfSome(ruleMessage => result.Add(ruleMessage));

            var disabledUniqueKeys = table.UniqueKeys.Where(uk => !uk.IsEnabled);
            foreach (var uniqueKey in disabledUniqueKeys)
            {
                var ruleMessage = BuildDisabledUniqueKeyMessage(table.Name, uniqueKey.Name);
                result.Add(ruleMessage);
            }

            var disabledChecks = table.Checks.Where(ck => !ck.IsEnabled);
            foreach (var check in disabledChecks)
            {
                var ruleMessage = BuildDisabledCheckConstraintMessage(table.Name, check.Name);
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

        protected virtual IRuleMessage BuildDisabledForeignKeyMessage(Identifier tableName, Option<Identifier> foreignKeyName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageKeyName = foreignKeyName.Match(
                name => " '" + name.LocalName + "'",
                () => string.Empty
            );

            var messageText = $"The table '{ tableName }' contains a disabled foreign key{ messageKeyName }. Consider enabling or removing the foreign key.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildDisabledPrimaryKeyMessage(Identifier tableName, Option<Identifier> primaryKeyName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageKeyName = primaryKeyName.Match(
                name => " '" + name.LocalName + "'",
                () => string.Empty
            );

            var messageText = $"The table '{ tableName }' contains a disabled primary key{ messageKeyName }. Consider enabling or removing the primary key.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildDisabledUniqueKeyMessage(Identifier tableName, Option<Identifier> uniqueKeyName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageKeyName = uniqueKeyName.Match(
                name => " '" + name.LocalName + "'",
                () => string.Empty
            );

            var messageText = $"The table '{ tableName }' contains a disabled unique key{ messageKeyName }. Consider enabling or removing the unique key.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildDisabledCheckConstraintMessage(Identifier tableName, Option<Identifier> checkName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageCheckName = checkName.Match(
                name => " '" + name.LocalName + "'",
                () => string.Empty
            );

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
