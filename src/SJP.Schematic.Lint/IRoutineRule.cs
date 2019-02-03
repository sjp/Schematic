using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint
{
    public interface IRoutineRule : IRule
    {
        IEnumerable<IRuleMessage> AnalyseRoutines(IEnumerable<IDatabaseRoutine> routines);

        Task<IEnumerable<IRuleMessage>> AnalyseRoutinesAsync(IEnumerable<IDatabaseRoutine> routines, CancellationToken cancellationToken = default(CancellationToken));
    }
}
