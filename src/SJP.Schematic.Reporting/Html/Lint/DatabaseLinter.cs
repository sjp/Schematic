using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Html.Lint
{
    internal sealed class DatabaseLinter
    {
        public DatabaseLinter(IDbConnection connection, IDatabaseDialect dialect, RuleLevel level = RuleLevel.Warning)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));

            if (!level.IsValid())
                throw new ArgumentException($"The { nameof(RuleLevel) } provided must be a valid enum.", nameof(level));
            Level = level;
        }

        private IDbConnection Connection { get; }

        private IDatabaseDialect Dialect { get; }

        private RuleLevel Level { get; }

        public Task<IEnumerable<IRuleMessage>> AnalyseTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return AnalyseTablesAsyncCore(tables, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseTablesAsyncCore(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken)
        {
            var result = new List<IRuleMessage>();

            foreach (var tableRule in TableRules)
            {
                var messages = await tableRule.AnalyseTablesAsync(tables, cancellationToken).ConfigureAwait(false);
                result.AddRange(messages);
            }

            return result;
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseViewsAsync(IEnumerable<IDatabaseView> views, CancellationToken cancellationToken)
        {
            if (views == null)
                throw new ArgumentNullException(nameof(views));

            return AnalyseViewsAsyncCore(views, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseViewsAsyncCore(IEnumerable<IDatabaseView> views, CancellationToken cancellationToken)
        {
            var result = new List<IRuleMessage>();

            foreach (var viewRule in ViewRules)
            {
                var messages = await viewRule.AnalyseViewsAsync(views, cancellationToken).ConfigureAwait(false);
                result.AddRange(messages);
            }

            return result;
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseSequencesAsync(IEnumerable<IDatabaseSequence> sequences, CancellationToken cancellationToken)
        {
            if (sequences == null)
                throw new ArgumentNullException(nameof(sequences));

            return AnalyseSequencesAsyncCore(sequences, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseSequencesAsyncCore(IEnumerable<IDatabaseSequence> sequences, CancellationToken cancellationToken)
        {
            var result = new List<IRuleMessage>();

            foreach (var sequenceRule in SequenceRules)
            {
                var messages = await sequenceRule.AnalyseSequencesAsync(sequences, cancellationToken).ConfigureAwait(false);
                result.AddRange(messages);
            }

            return result;
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseSynonymsAsync(IEnumerable<IDatabaseSynonym> synonyms, CancellationToken cancellationToken)
        {
            if (synonyms == null)
                throw new ArgumentNullException(nameof(synonyms));

            return AnalyseSynonymsAsyncCore(synonyms, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseSynonymsAsyncCore(IEnumerable<IDatabaseSynonym> synonyms, CancellationToken cancellationToken)
        {
            var result = new List<IRuleMessage>();

            foreach (var synonymRule in SynonymRules)
            {
                var messages = await synonymRule.AnalyseSynonymsAsync(synonyms, cancellationToken).ConfigureAwait(false);
                result.AddRange(messages);
            }

            return result;
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseRoutinesAsync(IEnumerable<IDatabaseRoutine> routines, CancellationToken cancellationToken)
        {
            if (routines == null)
                throw new ArgumentNullException(nameof(routines));

            return AnalyseRoutinesAsyncCore(routines, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseRoutinesAsyncCore(IEnumerable<IDatabaseRoutine> routines, CancellationToken cancellationToken)
        {
            var result = new List<IRuleMessage>();

            foreach (var routineRule in RoutineRules)
            {
                var messages = await routineRule.AnalyseRoutinesAsync(routines, cancellationToken).ConfigureAwait(false);
                result.AddRange(messages);
            }

            return result;
        }

        private IEnumerable<IRule> Rules => new IRule[]
        {
            new CandidateKeyMissingRule(Level),
            new ColumnWithNullDefaultValueRule(Level),
            new DisabledObjectsRule(Level),
            new ForeignKeyColumnTypeMismatchRule(Level),
            new ForeignKeyIndexRule(Level),
            new ForeignKeyIsPrimaryKeyRule(Level),
            new ForeignKeyRelationshipCycleRule(Level),
            new InvalidViewDefinitionRule(Connection, Dialect, Level),
            new NoNonNullableColumnsPresentRule(Level),
            new NoSurrogatePrimaryKeyRule(Level),
            new NoValueForNullableColumnRule(Connection, Dialect, Level),
            new OnlyOneColumnPresentRule(Level),
            new OrphanedTableRule(Level),
            new PrimaryKeyColumnNotFirstColumnRule(Level),
            new PrimaryKeyNotIntegerRule(Level),
            new RedundantIndexesRule(Level),
            new ReservedKeywordNameRule(Dialect, Level),
            new TooManyColumnsRule(Level),
            new UniqueIndexWithNullableColumnsRule(Level),
            new WhitespaceNameRule(Level)
        };

        private IEnumerable<ITableRule> TableRules => Rules.OfType<ITableRule>();
        private IEnumerable<IViewRule> ViewRules => Rules.OfType<IViewRule>();
        private IEnumerable<ISequenceRule> SequenceRules => Rules.OfType<ISequenceRule>();
        private IEnumerable<ISynonymRule> SynonymRules => Rules.OfType<ISynonymRule>();
        private IEnumerable<IRoutineRule> RoutineRules => Rules.OfType<IRoutineRule>();
    }
}
