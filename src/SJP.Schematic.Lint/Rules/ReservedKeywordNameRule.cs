using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class ReservedKeywordNameRule : Rule
    {
        public ReservedKeywordNameRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public override Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsync(IRelationalDatabase database, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (database.Dialect == null)
                throw new ArgumentException("The dialect on the given database is null.", nameof(database));

            return AnalyseDatabaseAsyncCore(database, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsyncCore(IRelationalDatabase database, CancellationToken cancellationToken)
        {
            var dialect = database.Dialect;

            var tables = await database.GetAllTables(cancellationToken).ConfigureAwait(false);
            var tableMessages = tables.SelectMany(t => AnalyseTable(dialect, t));

            var views = await database.GetAllViews(cancellationToken).ConfigureAwait(false);
            var viewMessages = views.SelectMany(v => AnalyseView(dialect, v));

            var sequences = await database.GetAllSequences(cancellationToken).ConfigureAwait(false);
            var sequenceMessages = sequences.SelectMany(s => AnalyseSequence(dialect, s));

            var synonyms = await database.GetAllSynonyms(cancellationToken).ConfigureAwait(false);
            var synonymMessages = synonyms.SelectMany(s => AnalyseSynonym(dialect, s));

            return tableMessages
                .Concat(viewMessages)
                .Concat(sequenceMessages)
                .Concat(synonymMessages)
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
                var message = BuildTableMessage(table.Name);
                result.Add(message);
            }

            var keywordColumnNames = table.Columns
                .Select(c => c.Name.LocalName)
                .Where(dialect.IsReservedKeyword);

            foreach (var kwColumnName in keywordColumnNames)
            {
                var message = BuildTableColumnMessage(table.Name, kwColumnName);
                result.Add(message);
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
                var message = BuildViewMessage(view.Name);
                result.Add(message);
            }

            var keywordColumnNames = view.Columns
                .Select(c => c.Name.LocalName)
                .Where(dialect.IsReservedKeyword);

            foreach (var kwColumnName in keywordColumnNames)
            {
                var message = BuildViewColumnMessage(view.Name, kwColumnName);
                result.Add(message);
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
                var message = BuildSequenceMessage(sequence.Name);
                result.Add(message);
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
                var message = BuildSynonymMessage(synonym.Name);
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

        protected static string RuleTitle { get; } = "Object name is a reserved keyword.";
    }
}
