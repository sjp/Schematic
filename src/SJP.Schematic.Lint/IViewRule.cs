using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint
{
    public interface IViewRule : IRule
    {
        IEnumerable<IRuleMessage> AnalyseViews(IEnumerable<IDatabaseView> views);

        Task<IEnumerable<IRuleMessage>> AnalyseViewsAsync(IEnumerable<IDatabaseView> views, CancellationToken cancellationToken = default);
    }
}
