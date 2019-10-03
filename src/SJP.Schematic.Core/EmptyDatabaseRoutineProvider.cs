using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public sealed class EmptyDatabaseRoutineProvider : IDatabaseRoutineProvider
    {
        public IAsyncEnumerable<IDatabaseRoutine> GetAllRoutines(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseRoutine>();

        public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return OptionAsync<IDatabaseRoutine>.None;
        }
    }
}
