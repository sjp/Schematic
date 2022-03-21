using System.Collections.Generic;
using System.Threading;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint;

/// <summary>
/// A rule that will analyse and report potential issues with database routines.
/// </summary>
/// <seealso cref="IRule" />
public interface IRoutineRule : IRule
{
    /// <summary>
    /// Analyses database routines.
    /// </summary>
    /// <param name="routines">A set of database routines.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    IAsyncEnumerable<IRuleMessage> AnalyseRoutines(IEnumerable<IDatabaseRoutine> routines, CancellationToken cancellationToken = default);
}