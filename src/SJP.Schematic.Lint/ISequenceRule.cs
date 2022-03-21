using System.Collections.Generic;
using System.Threading;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint;

/// <summary>
/// A rule that will analyse and report potential issues with database sequences.
/// </summary>
/// <seealso cref="IRule" />
public interface ISequenceRule : IRule
{
    /// <summary>
    /// Analyses database sequences.
    /// </summary>
    /// <param name="sequences">A set of database sequences.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    IAsyncEnumerable<IRuleMessage> AnalyseSequences(IEnumerable<IDatabaseSequence> sequences, CancellationToken cancellationToken = default);
}