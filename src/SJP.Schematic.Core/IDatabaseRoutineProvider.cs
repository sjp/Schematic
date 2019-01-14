using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IDatabaseRoutineProvider
    {
        OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines(CancellationToken cancellationToken = default(CancellationToken));
    }
}
