using System.Collections.Generic;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IDatabaseRoutineProvider
    {
        OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default);

        IAsyncEnumerable<IDatabaseRoutine> GetAllRoutines(CancellationToken cancellationToken = default);
    }
}
