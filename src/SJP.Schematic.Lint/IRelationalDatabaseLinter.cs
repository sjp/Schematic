using System.Collections.Generic;
using System.Threading;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint;

/// <summary>
/// Describes a linter that applies database linting rules to a set of database objects.
/// </summary>
public interface IRelationalDatabaseLinter
{
    /// <summary>
    /// Analyses a relational database.
    /// </summary>
    /// <param name="database">A relational database. Analysis will be performed on objects retrieved from the database.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    IAsyncEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyses database routines.
    /// </summary>
    /// <param name="routines">A set of database routines.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    IAsyncEnumerable<IRuleMessage> AnalyseRoutines(IEnumerable<IDatabaseRoutine> routines, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyses database sequences.
    /// </summary>
    /// <param name="sequences">A set of database sequences.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    IAsyncEnumerable<IRuleMessage> AnalyseSequences(IEnumerable<IDatabaseSequence> sequences, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyses database synonyms.
    /// </summary>
    /// <param name="synonyms">A set of database synonyms.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    IAsyncEnumerable<IRuleMessage> AnalyseSynonyms(IEnumerable<IDatabaseSynonym> synonyms, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyses database tables.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyses database views.
    /// </summary>
    /// <param name="views">A set of database views.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    IAsyncEnumerable<IRuleMessage> AnalyseViews(IEnumerable<IDatabaseView> views, CancellationToken cancellationToken = default);
}