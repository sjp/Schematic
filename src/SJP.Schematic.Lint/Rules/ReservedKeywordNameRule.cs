using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class ReservedKeywordNameRule : Rule
    {
        public ReservedKeywordNameRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public override IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            var dialect = database.Dialect;
            if (dialect == null)
                throw new ArgumentException("The dialect on the given database is null.", nameof(database));

            return database.Tables.SelectMany(t => AnalyseTable(dialect, t))
                .Concat(database.Views.SelectMany(v => AnalyseView(dialect, v)))
                .Concat(database.Sequences.SelectMany(s => AnalyseSequence(dialect, s)))
                .Concat(database.Synonyms.SelectMany(s => AnalyseSynonym(dialect, s)))
                .ToList();
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IDatabaseDialect dialect, IRelationalDatabaseTable table)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var result = new List<IRuleMessage>();

            var tableNameIsKeyword = dialect.IsReservedKeyword(table.Name.LocalName);
            if (tableNameIsKeyword)
            {
                var messageText = $"The table '{ table.Name }' is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            var keywordColumnNames = table.Columns
                .Select(c => c.Name.LocalName)
                .Where(dialect.IsReservedKeyword);

            foreach (var kwColumnName in keywordColumnNames)
            {
                var messageText = $"The table '{ table.Name }' contains a column '{ kwColumnName }' which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            return result;
        }

        protected IEnumerable<IRuleMessage> AnalyseView(IDatabaseDialect dialect, IRelationalDatabaseView view)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var result = new List<IRuleMessage>();

            var viewNameIsKeyword = dialect.IsReservedKeyword(view.Name.LocalName);
            if (viewNameIsKeyword)
            {
                var messageText = $"The view '{ view.Name }' is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            var keywordColumnNames = view.Columns
                .Select(c => c.Name.LocalName)
                .Where(dialect.IsReservedKeyword);

            foreach (var kwColumnName in keywordColumnNames)
            {
                var messageText = $"The view '{ view.Name }' contains a column '{ kwColumnName }' which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            return result;
        }

        protected IEnumerable<IRuleMessage> AnalyseSequence(IDatabaseDialect dialect, IDatabaseSequence sequence)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            var result = new List<IRuleMessage>();

            var sequenceNameIsKeyword = dialect.IsReservedKeyword(sequence.Name.LocalName);
            if (sequenceNameIsKeyword)
            {
                var messageText = $"The sequence '{ sequence.Name }' is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            return result;
        }

        protected IEnumerable<IRuleMessage> AnalyseSynonym(IDatabaseDialect dialect, IDatabaseSynonym synonym)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));

            var result = new List<IRuleMessage>();

            var synonymNameIsKeyword = dialect.IsReservedKeyword(synonym.Name.LocalName);
            if (synonymNameIsKeyword)
            {
                var messageText = $"The synonym '{ synonym.Name }' is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                result.Add(ruleMessage);
            }

            return result;
        }

        private const string RuleTitle = "Object name is a reserved keyword.";
    }
}
