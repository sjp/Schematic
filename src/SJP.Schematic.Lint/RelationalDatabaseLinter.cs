using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

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
    public IAsyncEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);

        return AnalyseDatabaseCore(database, cancellationToken);
    }

    private async IAsyncEnumerable<IRuleMessage> AnalyseDatabaseCore(IRelationalDatabase database, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var tables = await database.GetAllTables(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var tableRule in TableRules)
        {
            await foreach (var message in tableRule.AnalyseTables(tables, cancellationToken).ConfigureAwait(false).WithCancellation(cancellationToken))
                yield return message;
        }

        var views = await database.GetAllViews(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var viewRule in ViewRules)
        {
            await foreach (var message in viewRule.AnalyseViews(views, cancellationToken).ConfigureAwait(false).WithCancellation(cancellationToken))
                yield return message;
        }

        var sequences = await database.GetAllSequences(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var sequenceRule in SequenceRules)
        {
            await foreach (var message in sequenceRule.AnalyseSequences(sequences, cancellationToken).ConfigureAwait(false).WithCancellation(cancellationToken))
                yield return message;
        }

        var synonyms = await database.EnumerateAllSynonyms(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var synonymRule in SynonymRules)
        {
            await foreach (var message in synonymRule.AnalyseSynonyms(synonyms, cancellationToken).ConfigureAwait(false).WithCancellation(cancellationToken))
                yield return message;
        }

        var routines = await database.EnumerateAllRoutines(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var routineRule in RoutineRules)
        {
            await foreach (var message in routineRule.AnalyseRoutines(routines, cancellationToken).ConfigureAwait(false).WithCancellation(cancellationToken))
                yield return message;
        }
    }

    /// <summary>
    /// Analyses database tables.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        return AnalyseTablesCore(tables, cancellationToken);
    }

    private async IAsyncEnumerable<IRuleMessage> AnalyseTablesCore(IEnumerable<IRelationalDatabaseTable> tables, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var tableRule in TableRules)
        {
            await foreach (var message in tableRule.AnalyseTables(tables, cancellationToken).ConfigureAwait(false).WithCancellation(cancellationToken))
                yield return message;
        }
    }

    /// <summary>
    /// Analyses database views.
    /// </summary>
    /// <param name="views">A set of database views.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="views"/> is <see langword="null" />.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseViews(IReadOnlyCollection<IDatabaseView> views, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(views);

        return AnalyseViewsCore(views, cancellationToken);
    }

    private async IAsyncEnumerable<IRuleMessage> AnalyseViewsCore(IEnumerable<IDatabaseView> views, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var viewRule in ViewRules)
        {
            await foreach (var message in viewRule.AnalyseViews(views, cancellationToken).ConfigureAwait(false).WithCancellation(cancellationToken))
                yield return message;
        }
    }

    /// <summary>
    /// Analyses database sequences.
    /// </summary>
    /// <param name="sequences">A set of database sequences.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequences"/> is <see langword="null" />.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseSequences(IReadOnlyCollection<IDatabaseSequence> sequences, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequences);

        return AnalyseSequencesCore(sequences, cancellationToken);
    }

    private async IAsyncEnumerable<IRuleMessage> AnalyseSequencesCore(IEnumerable<IDatabaseSequence> sequences, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var sequenceRule in SequenceRules)
        {
            await foreach (var message in sequenceRule.AnalyseSequences(sequences, cancellationToken).ConfigureAwait(false).WithCancellation(cancellationToken))
                yield return message;
        }
    }

    /// <summary>
    /// Analyses database synonyms.
    /// </summary>
    /// <param name="synonyms">A set of database synonyms.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonyms"/> is <see langword="null" />.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseSynonyms(IReadOnlyCollection<IDatabaseSynonym> synonyms, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonyms);

        return AnalyseSynonymsCore(synonyms, cancellationToken);
    }

    private async IAsyncEnumerable<IRuleMessage> AnalyseSynonymsCore(IEnumerable<IDatabaseSynonym> synonyms, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var synonymRule in SynonymRules)
        {
            await foreach (var message in synonymRule.AnalyseSynonyms(synonyms, cancellationToken).ConfigureAwait(false).WithCancellation(cancellationToken))
                yield return message;
        }
    }

    /// <summary>
    /// Analyses database routines.
    /// </summary>
    /// <param name="routines">A set of database routines.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routines"/> is <see langword="null" />.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseRoutines(IReadOnlyCollection<IDatabaseRoutine> routines, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routines);

        return AnalyseRoutinesCore(routines, cancellationToken);
    }

    private async IAsyncEnumerable<IRuleMessage> AnalyseRoutinesCore(IEnumerable<IDatabaseRoutine> routines, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var routineRule in RoutineRules)
        {
            await foreach (var message in routineRule.AnalyseRoutines(routines, cancellationToken).ConfigureAwait(false).WithCancellation(cancellationToken))
                yield return message;
        }
    }
}