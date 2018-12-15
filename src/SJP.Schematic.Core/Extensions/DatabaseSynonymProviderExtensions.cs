using System;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Extensions
{
    public static class DatabaseSynonymProviderExtensions
    {
        public static bool TryGetSynonym(this IDatabaseSynonymProvider synonymProvider, Identifier synonymName, out IDatabaseSynonym synonym)
        {
            if (synonymProvider == null)
                throw new ArgumentNullException(nameof(synonymProvider));
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var synonymOption = TryGetSynonymAsyncCore(synonymProvider, synonymName, CancellationToken.None).GetAwaiter().GetResult();
            synonym = synonymOption.synonym;

            return synonymOption.exists;
        }

        public static Task<(bool exists, IDatabaseSynonym synonym)> TryGetSynonymAsync(this IDatabaseSynonymProvider synonymProvider, Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymProvider == null)
                throw new ArgumentNullException(nameof(synonymProvider));
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return TryGetSynonymAsyncCore(synonymProvider, synonymName, cancellationToken);
        }

        private static async Task<(bool exists, IDatabaseSynonym synonym)> TryGetSynonymAsyncCore(IDatabaseSynonymProvider synonymProvider, Identifier synonymName, CancellationToken cancellationToken)
        {
            var synonymOption = synonymProvider.GetSynonym(synonymName, cancellationToken);
            var exists = await synonymOption.IsSome.ConfigureAwait(false);
            var synonym = await synonymOption.IfNoneUnsafe(default(IDatabaseSynonym)).ConfigureAwait(false);

            return (exists, synonym);
        }
    }
}
