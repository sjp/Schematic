using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint;

/// <summary>
/// A rule that will analyse and report potential issues with database tables.
/// </summary>
/// <seealso cref="IRule" />
public interface ITableRule : IRule
{
    /// <summary>
    /// Analyses database tables.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    Task<IReadOnlyCollection<IRuleMessage>> AnalyseTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default);
}