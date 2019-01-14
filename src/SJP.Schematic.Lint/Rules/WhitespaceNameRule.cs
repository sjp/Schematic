using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class WhitespaceNameRule : Rule
    {
        public WhitespaceNameRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public override Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsync(IRelationalDatabase database, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return AnalyseDatabaseAsyncCore(database, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsyncCore(IRelationalDatabase database, CancellationToken cancellationToken)
        {
            var tables = await database.GetAllTables(cancellationToken).ConfigureAwait(false);
            var views = await database.GetAllViews(cancellationToken).ConfigureAwait(false);
            var sequences = await database.GetAllSequences(cancellationToken).ConfigureAwait(false);
            var synonyms = await database.GetAllSynonyms(cancellationToken).ConfigureAwait(false);
            var routines = await database.GetAllRoutines(cancellationToken).ConfigureAwait(false);

            return tables.SelectMany(AnalyseTable)
                .Concat(views.SelectMany(AnalyseView))
                .Concat(sequences.SelectMany(AnalyseSequence))
                .Concat(synonyms.SelectMany(AnalyseSynonym))
                .Concat(routines.SelectMany(AnalyseRoutine))
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
