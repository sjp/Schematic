using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    public sealed class EmptyDatabaseRoutineProvider : IDatabaseRoutineProvider
    {
        public Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines(CancellationToken cancellationToken = default(CancellationToken)) => Empty.Routines;

        public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return OptionAsync<IDatabaseRoutine>.None;
        }
    }
}
