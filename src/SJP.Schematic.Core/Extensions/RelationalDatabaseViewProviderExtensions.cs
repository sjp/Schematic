using System;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Extensions
{
    public static class RelationalDatabaseViewProviderExtensions
    {
        public static bool TryGetView(this IRelationalDatabaseViewProvider viewProvider, Identifier viewName, out IRelationalDatabaseView view)
        {
            if (viewProvider == null)
                throw new ArgumentNullException(nameof(viewProvider));
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var viewOption = TryGetViewAsyncCore(viewProvider, viewName, CancellationToken.None).GetAwaiter().GetResult();
            view = viewOption.view;

            return viewOption.exists;
        }

        public static Task<(bool exists, IRelationalDatabaseView view)> TryGetViewAsync(this IRelationalDatabaseViewProvider viewProvider, Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewProvider == null)
                throw new ArgumentNullException(nameof(viewProvider));
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return TryGetViewAsyncCore(viewProvider, viewName, cancellationToken);
        }

        private static async Task<(bool exists, IRelationalDatabaseView view)> TryGetViewAsyncCore(IRelationalDatabaseViewProvider viewProvider, Identifier viewName, CancellationToken cancellationToken)
        {
            var viewOption = viewProvider.GetView(viewName, cancellationToken);
            var exists = await viewOption.IsSome.ConfigureAwait(false);
            var view = await viewOption.IfNoneUnsafe(default(IRelationalDatabaseView)).ConfigureAwait(false);

            return (exists, view);
        }
    }
}
