using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint;

/// <summary>
/// A rule that will analyse and report potential issues with database views.
/// </summary>
/// <seealso cref="IRule" />
public interface IViewRule : IRule
{
    /// <summary>
    /// Analyses database views.
    /// </summary>
    /// <param name="views">A set of database views.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    Task<IReadOnlyCollection<IRuleMessage>> AnalyseViews(IReadOnlyCollection<IDatabaseView> views, CancellationToken cancellationToken = default);
}