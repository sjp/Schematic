using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return AnalyseTablesCore(tables, cancellationToken);
        }

        private async IAsyncEnumerable<IRuleMessage> AnalyseTablesCore(IEnumerable<IRelationalDatabaseTable> tables, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var tableRule in TableRules)
            {
                await foreach (var message in tableRule.AnalyseTables(tables, cancellationToken).ConfigureAwait(false))
                    yield return message;
            }
        }

        public IAsyncEnumerable<IRuleMessage> AnalyseViews(IEnumerable<IDatabaseView> views, CancellationToken cancellationToken)
        {
            if (views == null)
                throw new ArgumentNullException(nameof(views));

            return AnalyseViewsCore(views, cancellationToken);
        }

        private async IAsyncEnumerable<IRuleMessage> AnalyseViewsCore(IEnumerable<IDatabaseView> views, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var viewRule in ViewRules)
            {
                await foreach (var message in viewRule.AnalyseViews(views, cancellationToken).ConfigureAwait(false))
                    yield return message;
            }
        }

        public IAsyncEnumerable<IRuleMessage> AnalyseSequences(IEnumerable<IDatabaseSequence> sequences, CancellationToken cancellationToken)
        {
            if (sequences == null)
                throw new ArgumentNullException(nameof(sequences));

            return AnalyseSequencesCore(sequences, cancellationToken);
        }

        private async IAsyncEnumerable<IRuleMessage> AnalyseSequencesCore(IEnumerable<IDatabaseSequence> sequences, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var sequenceRule in SequenceRules)
            {
                await foreach (var message in sequenceRule.AnalyseSequences(sequences, cancellationToken).ConfigureAwait(false))
                    yield return message;
            }
        }

        public IAsyncEnumerable<IRuleMessage> AnalyseSynonyms(IEnumerable<IDatabaseSynonym> synonyms, CancellationToken cancellationToken)
        {
            if (synonyms == null)
                throw new ArgumentNullException(nameof(synonyms));

            return AnalyseSynonymsCore(synonyms, cancellationToken);
        }

        private async IAsyncEnumerable<IRuleMessage> AnalyseSynonymsCore(IEnumerable<IDatabaseSynonym> synonyms, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var synonymRule in SynonymRules)
            {
                await foreach (var message in synonymRule.AnalyseSynonyms(synonyms, cancellationToken).ConfigureAwait(false))
                    yield return message;
            }
        }

        public IAsyncEnumerable<IRuleMessage> AnalyseRoutines(IEnumerable<IDatabaseRoutine> routines, CancellationToken cancellationToken)
        {
            if (routines == null)
                throw new ArgumentNullException(nameof(routines));

            return AnalyseRoutinesCore(routines, cancellationToken);
        }

        private async IAsyncEnumerable<IRuleMessage> AnalyseRoutinesCore(IEnumerable<IDatabaseRoutine> routines, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var routineRule in RoutineRules)
            {
                await foreach (var message in routineRule.AnalyseRoutines(routines, cancellationToken).ConfigureAwait(false))
                    yield return message;
            }
        }

        private IEnumerable<IRule> Rules => new IRule[]
        {
            new CandidateKeyMissingRule(Level),
            new ColumnWithNullDefaultValueRule(Level),
            new DisabledObjectsRule(Level),
            new ForeignKeyColumnTypeMismatchRule(Level),
            new ForeignKeyIndexRule(Level),
            new ForeignKeyIsPrimaryKeyRule(Level),
            new ForeignKeyMissingRule(Level),
            new ForeignKeyRelationshipCycleRule(Level),
            new InvalidViewDefinitionRule(Connection, Dialect, Level),
            new NoIndexesPresentOnTableRule(Level),
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
