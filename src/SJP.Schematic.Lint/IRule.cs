using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint
{
    public interface IRule
    {
        RuleLevel Level { get; }

        string Title { get; }

        IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database);

        Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsync(IRelationalDatabase database, CancellationToken cancellationToken = default(CancellationToken));
    }
}
