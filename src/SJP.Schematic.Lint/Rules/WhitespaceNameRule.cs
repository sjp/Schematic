using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class WhitespaceNameRule : Rule
    {
        public WhitespaceNameRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public override IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return database.Tables.SelectMany(AnalyseTable)
                .Concat(database.Views.SelectMany(AnalyseView))
                .Concat(database.Sequences.SelectMany(AnalyseSequence))
                .Concat(database.Synonyms.SelectMany(AnalyseSynonym))
                .ToList();
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var result = new List<IRuleMessage>();

            var tableNameHasWs = HasWhiteSpace(table.Name.LocalName);
            if (tableNameHasWs)
            {
                var messageText = $"The table '{ table.Name }' contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            var whiteSpaceColumnNames = table.Columns
                .Select(c => c.Name.LocalName)
                .Where(HasWhiteSpace);

            foreach (var wsColumnName in whiteSpaceColumnNames)
            {
                var messageText = $"The table '{ table.Name }' contains a column '{ wsColumnName }' which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            return result;
        }

        protected IEnumerable<IRuleMessage> AnalyseView(IRelationalDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var result = new List<IRuleMessage>();

            var viewNameHasWs = HasWhiteSpace(view.Name.LocalName);
            if (viewNameHasWs)
            {
                var messageText = $"The view '{ view.Name }' contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            var whiteSpaceColumnNames = view.Columns
                .Select(c => c.Name.LocalName)
                .Where(HasWhiteSpace);

            foreach (var wsColumnName in whiteSpaceColumnNames)
            {
                var messageText = $"The view '{ view.Name }' contains a column '{ wsColumnName }' which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            return result;
        }

        protected IEnumerable<IRuleMessage> AnalyseSequence(IDatabaseSequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            var result = new List<IRuleMessage>();

            var sequenceNameHasWs = HasWhiteSpace(sequence.Name.LocalName);
            if (sequenceNameHasWs)
            {
                var messageText = $"The sequence '{ sequence.Name }' contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            return result;
        }

        protected IEnumerable<IRuleMessage> AnalyseSynonym(IDatabaseSynonym synonym)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));

            var result = new List<IRuleMessage>();

            var synonymNameHasWs = HasWhiteSpace(synonym.Name.LocalName);
            if (synonymNameHasWs)
            {
                var messageText = $"The synonym '{ synonym.Name }' contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            return result;
        }

        private static bool HasWhiteSpace(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return input.Any(char.IsWhiteSpace);
        }

        private const string RuleTitle = "Whitespace present in object name.";
    }
}
