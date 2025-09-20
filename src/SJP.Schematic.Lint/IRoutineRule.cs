using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    Task<IReadOnlyCollection<IRuleMessage>> AnalyseRoutines(IReadOnlyCollection<IDatabaseRoutine> routines, CancellationToken cancellationToken = default);
}