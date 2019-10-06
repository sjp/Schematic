using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class ReservedKeywordNameRule : Rule, ITableRule, IViewRule, ISequenceRule, ISynonymRule, IRoutineRule
    {
        public ReservedKeywordNameRule(IDatabaseDialect dialect, RuleLevel level)
            : base(RuleTitle, level)
        {
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        protected IDatabaseDialect Dialect { get; }

        public IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return tables.SelectMany(AnalyseTable).ToAsyncEnumerable();
        }

        public IAsyncEnumerable<IRuleMessage> AnalyseViews(IEnumerable<IDatabaseView> views, CancellationToken cancellationToken = default)
        {
            if (views == null)
                throw new ArgumentNullException(nameof(views));

            return views.SelectMany(AnalyseView).ToAsyncEnumerable();
        }

        public IAsyncEnumerable<IRuleMessage> AnalyseSequences(IEnumerable<IDatabaseSequence> sequences, CancellationToken cancellationToken = default)
        {
            if (sequences == null)
                throw new ArgumentNullException(nameof(sequences));

            return sequences.SelectMany(AnalyseSequence).ToAsyncEnumerable();
        }

        public IAsyncEnumerable<IRuleMessage> AnalyseSynonyms(IEnumerable<IDatabaseSynonym> synonyms, CancellationToken cancellationToken = default)
        {
            if (synonyms == null)
                throw new ArgumentNullException(nameof(synonyms));

            return synonyms.SelectMany(AnalyseSynonym).ToAsyncEnumerable();
        }

        public IAsyncEnumerable<IRuleMessage> AnalyseRoutines(IEnumerable<IDatabaseRoutine> routines, CancellationToken cancellationToken = default)
        {
            if (routines == null)
                throw new ArgumentNullException(nameof(routines));

            return routines.SelectMany(AnalyseRoutine).ToAsyncEnumerable();
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var result = new List<IRuleMessage>();

            var tableNameIsKeyword = Dialect.IsReservedKeyword(table.Name.LocalName);
            if (tableNameIsKeyword)
            {
                var message = BuildTableMessage(table.Name);
                result.Add(message);
            }

            var keywordColumnNames = table.Columns
                .Select(c => c.Name.LocalName)
                .Where(Dialect.IsReservedKeyword);

            foreach (var kwColumnName in keywordColumnNames)
            {
                var message = BuildTableColumnMessage(table.Name, kwColumnName);
                result.Add(message);
            }

            return result;
        }

        protected IEnumerable<IRuleMessage> AnalyseView(IDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var result = new List<IRuleMessage>();

            var viewNameIsKeyword = Dialect.IsReservedKeyword(view.Name.LocalName);
            if (viewNameIsKeyword)
            {
                var message = BuildViewMessage(view.Name);
                result.Add(message);
            }

            var keywordColumnNames = view.Columns
                .Select(c => c.Name.LocalName)
                .Where(Dialect.IsReservedKeyword);

            foreach (var kwColumnName in keywordColumnNames)
            {
                var message = BuildViewColumnMessage(view.Name, kwColumnName);
                result.Add(message);
            }

            return result;
        }

        protected IEnumerable<IRuleMessage> AnalyseSequence(IDatabaseSequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            var result = new List<IRuleMessage>();

            var sequenceNameIsKeyword = Dialect.IsReservedKeyword(sequence.Name.LocalName);
            if (sequenceNameIsKeyword)
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

            var synonymNameIsKeyword = Dialect.IsReservedKeyword(synonym.Name.LocalName);
            if (synonymNameIsKeyword)
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

            var routineNameIsKeyword = Dialect.IsReservedKeyword(routine.Name.LocalName);
            if (routineNameIsKeyword)
            {
                var message = BuildRoutineMessage(routine.Name);
                result.Add(message);
            }

            return result;
        }

        protected virtual IRuleMessage BuildTableMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageText = $"The table '{ tableName }' is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildTableColumnMessage(Identifier tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var messageText = $"The table '{ tableName }' contains a column '{ columnName }' which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildViewMessage(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var messageText = $"The view '{ viewName }' is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildViewColumnMessage(Identifier viewName, string columnName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var messageText = $"The view '{ viewName }' contains a column '{ columnName }' which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildSequenceMessage(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var messageText = $"The sequence '{ sequenceName }' is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildSynonymMessage(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var messageText = $"The synonym '{ synonymName }' is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected virtual IRuleMessage BuildRoutineMessage(Identifier routineName)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var messageText = $"The routine '{ routineName }' is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Object name is a reserved keyword.";
    }
}
