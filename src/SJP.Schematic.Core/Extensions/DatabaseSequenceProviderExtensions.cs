using System;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Extensions
{
    public static class DatabaseSequenceProviderExtensions
    {
        public static Task<(bool exists, IDatabaseSequence? sequence)> TryGetSequenceAsync(this IDatabaseSequenceProvider sequenceProvider, Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceProvider == null)
                throw new ArgumentNullException(nameof(sequenceProvider));
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return TryGetSequenceAsyncCore(sequenceProvider, sequenceName, cancellationToken);
        }

        private static async Task<(bool exists, IDatabaseSequence? sequence)> TryGetSequenceAsyncCore(IDatabaseSequenceProvider sequenceProvider, Identifier sequenceName, CancellationToken cancellationToken)
        {
            var sequenceOption = sequenceProvider.GetSequence(sequenceName, cancellationToken);
            var exists = await sequenceOption.IsSome.ConfigureAwait(false);
            var sequence = await sequenceOption.IfNoneUnsafe(default(IDatabaseSequence)!).ConfigureAwait(false);

            return (exists, sequence);
        }
    }
}
