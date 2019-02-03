using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class WhitespaceNameRule : Rule, ITableRule, IViewRule, ISequenceRule, ISynonymRule, IRoutineRule
    {
        public WhitespaceNameRule(RuleLevel level)
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

        public IEnumerable<IRuleMessage> AnalyseViews(IEnumerable<IDatabaseView> views)
        {
            if (views == null)
                throw new ArgumentNullException(nameof(views));

            return views.SelectMany(AnalyseView).ToList();
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseViewsAsync(IEnumerable<IDatabaseView> views, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (views == null)
                throw new ArgumentNullException(nameof(views));

            var messages = AnalyseViews(views);
            return Task.FromResult(messages);
        }

        public IEnumerable<IRuleMessage> AnalyseSequences(IEnumerable<IDatabaseSequence> sequences)
        {
            if (sequences == null)
                throw new ArgumentNullException(nameof(sequences));

            return sequences.SelectMany(AnalyseSequence).ToList();
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseSequencesAsync(IEnumerable<IDatabaseSequence> sequences, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequences == null)
                throw new ArgumentNullException(nameof(sequences));

            var messages = AnalyseSequences(sequences);
            return Task.FromResult(messages);
        }

        public IEnumerable<IRuleMessage> AnalyseSynonyms(IEnumerable<IDatabaseSynonym> synonyms)
        {
            if (synonyms == null)
                throw new ArgumentNullException(nameof(synonyms));

            return synonyms.SelectMany(AnalyseSynonym).ToList();
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseSynonymsAsync(IEnumerable<IDatabaseSynonym> synonyms, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonyms == null)
                throw new ArgumentNullException(nameof(synonyms));

            var messages = AnalyseSynonyms(synonyms);
            return Task.FromResult(messages);
        }

        public IEnumerable<IRuleMessage> AnalyseRoutines(IEnumerable<IDatabaseRoutine> routines)
        {
            if (routines == null)
                throw new ArgumentNullException(nameof(routines));

            return routines.SelectMany(AnalyseRoutine).ToList();
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseRoutinesAsync(IEnumerable<IDatabaseRoutine> routines, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routines == null)
                throw new ArgumentNullException(nameof(routines));

            var messages = AnalyseRoutines(routines);
            return Task.FromResult(messages);
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var result = new List<IRuleMessage>();

            var tableNameHasWs = HasWhiteSpace(table.Name.LocalName);
            if (tableNameHasWs)
            {
                var message = BuildTableMessage(table.Name);
                result.Add(message);
            }

            var whiteSpaceColumnNames = table.Columns
                .Select(c => c.Name.LocalName)
                .Where(HasWhiteSpace);

            foreach (var wsColumnName in whiteSpaceColumnNames)
            {
                var message = BuildTableColumnMessage(table.Name, wsColumnName);
                result.Add(message);
            }

            return result;
        }

        protected IEnumerable<IRuleMessage> AnalyseView(IDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var result = new List<IRuleMessage>();

            var viewNameHasWs = HasWhiteSpace(view.Name.LocalName);
            if (viewNameHasWs)
            {
                var message = BuildViewMessage(view.Name);
                result.Add(message);
            }

            var whiteSpaceColumnNames = view.Columns
                .Select(c => c.Name.LocalName)
                .Where(HasWhiteSpace);

            foreach (var wsColumnName in whiteSpaceColumnNames)
            {
                var message = BuildViewColumnMessage(view.Name, wsColumnName);
                result.Add(message);
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
                var message = BuildSequenceMessage(sequence.Name);
                result.Add(message);
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
                var message = BuildSynonymMessage(synonym.Name);
                result.Add(message);
            }

            return result;
        }

        protected IEnumerable<IRuleMessage> AnalyseRoutine(IDatabaseRoutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            var result = new List<IRuleMessage>();

            var routineNameHasWs = HasWhiteSpace(routine.Name.LocalName);
            if (routineNameHasWs)
            {
                var message = BuildRoutineMessage(routine.Name);
                result.Add(message);
            }

            return result;
        }

        private static bool HasWhiteSpace(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return input.Any(char.IsWhiteSpace);
        }

        protected virtual IRuleMessage BuildTableMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageText = $"The table '{ tableName }' contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildTableColumnMessage(Identifier tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var messageText = $"The table '{ tableName }' contains a column '{ columnName }' which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildViewMessage(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var messageText = $"The view '{ viewName }' contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildViewColumnMessage(Identifier viewName, string columnName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var messageText = $"The view '{ viewName }' contains a column '{ columnName }' which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildSequenceMessage(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var messageText = $"The sequence '{ sequenceName }' contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildSynonymMessage(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var messageText = $"The synonym '{ synonymName }' contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildRoutineMessage(Identifier routineName)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var messageText = $"The routine '{ routineName }' contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Whitespace present in object name.";
    }
}
