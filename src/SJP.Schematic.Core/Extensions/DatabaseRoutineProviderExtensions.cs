using System;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Extensions
{
    public static class DatabaseRoutineProviderExtensions
    {
        public static Task<(bool exists, IDatabaseRoutine? routine)> TryGetRoutineAsync(this IDatabaseRoutineProvider routineProvider, Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineProvider == null)
                throw new ArgumentNullException(nameof(routineProvider));
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return TryGetRoutineAsyncCore(routineProvider, routineName, cancellationToken);
        }

        private static async Task<(bool exists, IDatabaseRoutine? routine)> TryGetRoutineAsyncCore(IDatabaseRoutineProvider routineProvider, Identifier routineName, CancellationToken cancellationToken)
        {
            var routineOption = routineProvider.GetRoutine(routineName, cancellationToken);
            var exists = await routineOption.IsSome.ConfigureAwait(false);
            var routine = await routineOption.IfNoneUnsafe(default(IDatabaseRoutine)!).ConfigureAwait(false);

            return (exists, routine);
        }
    }
}
