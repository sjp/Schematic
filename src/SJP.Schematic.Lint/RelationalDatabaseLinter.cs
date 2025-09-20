using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint;

/// <summary>
/// A linter that applies database linting rules to a set of database objects.
/// </summary>
/// <seealso cref="IRelationalDatabaseLinter" />
public class RelationalDatabaseLinter : IRelationalDatabaseLinter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelationalDatabaseLinter"/> class.
    /// </summary>
    /// <param name="rules">A set of database linting rules.</param>
    /// <exception cref="ArgumentNullException"><paramref name="rules"/> is <see langword="null" />.</exception>
    public RelationalDatabaseLinter(IEnumerable<IRule> rules)
    {
        Rules = rules ?? throw new ArgumentNullException(nameof(rules));
    }

    /// <summary>
    /// Database linting rules.
    /// </summary>
    /// <value>The set of rules used to analyse database objects.</value>
    protected IEnumerable<IRule> Rules { get; }

    private IEnumerable<ITableRule> TableRules => Rules.OfType<ITableRule>();
    private IEnumerable<IViewRule> ViewRules => Rules.OfType<IViewRule>();
    private IEnumerable<ISequenceRule> SequenceRules => Rules.OfType<ISequenceRule>();
    private IEnumerable<ISynonymRule> SynonymRules => Rules.OfType<ISynonymRule>();
    private IEnumerable<IRoutineRule> RoutineRules => Rules.OfType<IRoutineRule>();

    /// <summary>
    /// Analyses a relational database.
    /// </summary>
    /// <param name="database">A relational database. Analysis will be performed on objects retrieved from the database.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="database"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseDatabase(IRelationalDatabase database, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);

        return AnalyseDatabaseCore(database, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IRuleMessage>> AnalyseDatabaseCore(IRelationalDatabase database, CancellationToken cancellationToken)
    {
        var (
            tables,
            views,
            sequences,
            synonyms,
            routines
        ) = await (
            database.GetAllTables(cancellationToken),
            database.GetAllViews(cancellationToken),
            database.GetAllSequences(cancellationToken),
            database.GetAllSynonyms(cancellationToken),
            database.GetAllRoutines(cancellationToken)
        ).WhenAll().ConfigureAwait(false);

        var (
            tableMessagesByRule,
            viewMessagesByRule,
            sequenceMessagesByRule,
            synonymMessagesByRule,
            routineMessagesByRule
        ) = await (
            TableRules.Select(tr => tr.AnalyseTables(tables, cancellationToken)).ToArray().WhenAll(),
            ViewRules.Select(vr => vr.AnalyseViews(views, cancellationToken)).ToArray().WhenAll(),
            SequenceRules.Select(sr => sr.AnalyseSequences(sequences, cancellationToken)).ToArray().WhenAll(),
            SynonymRules.Select(sr => sr.AnalyseSynonyms(synonyms, cancellationToken)).ToArray().WhenAll(),
            RoutineRules.Select(rr => rr.AnalyseRoutines(routines, cancellationToken)).ToArray().WhenAll()
        ).WhenAll().ConfigureAwait(false);

        // all evaluated, now to flatten + aggregate
        return tableMessagesByRule.SelectMany(_ => _)
            .Concat(viewMessagesByRule.SelectMany(_ => _))
            .Concat(sequenceMessagesByRule.SelectMany(_ => _))
            .Concat(synonymMessagesByRule.SelectMany(_ => _))
            .Concat(routineMessagesByRule.SelectMany(_ => _))
            .ToList();
    }

    /// <summary>
    /// Analyses database tables.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        return AnalyseTablesCore(tables, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IRuleMessage>> AnalyseTablesCore(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken)
    {
        var messages = await TableRules
            .Select(tr => tr.AnalyseTables(tables, cancellationToken))
            .ToArray()
            .WhenAll()
            .ConfigureAwait(false);

        return messages
            .SelectMany(_ => _)
            .ToList();
    }

    /// <summary>
    /// Analyses database views.
    /// </summary>
    /// <param name="views">A set of database views.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="views"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseViews(IReadOnlyCollection<IDatabaseView> views, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(views);

        return AnalyseViewsCore(views, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IRuleMessage>> AnalyseViewsCore(IReadOnlyCollection<IDatabaseView> views, CancellationToken cancellationToken)
    {
        var messages = await ViewRules
            .Select(vr => vr.AnalyseViews(views, cancellationToken))
            .ToArray()
            .WhenAll()
            .ConfigureAwait(false);

        return messages
            .SelectMany(_ => _)
            .ToList();
    }

    /// <summary>
    /// Analyses database sequences.
    /// </summary>
    /// <param name="sequences">A set of database sequences.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequences"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseSequences(IReadOnlyCollection<IDatabaseSequence> sequences, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequences);

        return AnalyseSequencesCore(sequences, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IRuleMessage>> AnalyseSequencesCore(IReadOnlyCollection<IDatabaseSequence> sequences, CancellationToken cancellationToken)
    {
        var messages = await SequenceRules
            .Select(sr => sr.AnalyseSequences(sequences, cancellationToken))
            .ToArray()
            .WhenAll()
            .ConfigureAwait(false);

        return messages
            .SelectMany(_ => _)
            .ToList();
    }

    /// <summary>
    /// Analyses database synonyms.
    /// </summary>
    /// <param name="synonyms">A set of database synonyms.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonyms"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseSynonyms(IReadOnlyCollection<IDatabaseSynonym> synonyms, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonyms);

        return AnalyseSynonymsCore(synonyms, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IRuleMessage>> AnalyseSynonymsCore(IReadOnlyCollection<IDatabaseSynonym> synonyms, CancellationToken cancellationToken)
    {
        var messages = await SynonymRules
            .Select(sr => sr.AnalyseSynonyms(synonyms, cancellationToken))
            .ToArray()
            .WhenAll()
            .ConfigureAwait(false);

        return messages
            .SelectMany(_ => _)
            .ToList();
    }

    /// <summary>
    /// Analyses database routines.
    /// </summary>
    /// <param name="routines">A set of database routines.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routines"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseRoutines(IReadOnlyCollection<IDatabaseRoutine> routines, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routines);

        return AnalyseRoutinesCore(routines, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IRuleMessage>> AnalyseRoutinesCore(IReadOnlyCollection<IDatabaseRoutine> routines, CancellationToken cancellationToken)
    {
        var messages = await RoutineRules
            .Select(rr => rr.AnalyseRoutines(routines, cancellationToken))
            .ToArray()
            .WhenAll()
            .ConfigureAwait(false);

        return messages
            .SelectMany(_ => _)
            .ToList();
    }
}