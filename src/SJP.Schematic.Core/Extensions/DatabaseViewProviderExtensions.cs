using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Extensions
{
    public static class DatabaseViewProviderExtensions
    {
        public static bool TryGetView(this IDatabaseViewProvider viewProvider, Identifier viewName, [NotNullWhen(true)] out IDatabaseView? view)
        {
            if (viewProvider == null)
                throw new ArgumentNullException(nameof(viewProvider));
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var viewOption = TryGetViewAsyncCore(viewProvider, viewName, CancellationToken.None).GetAwaiter().GetResult();
            view = viewOption.view;

            return viewOption.exists;
        }

        public static Task<(bool exists, IDatabaseView? view)> TryGetViewAsync(this IDatabaseViewProvider viewProvider, Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewProvider == null)
                throw new ArgumentNullException(nameof(viewProvider));
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return TryGetViewAsyncCore(viewProvider, viewName, cancellationToken);
        }

        private static async Task<(bool exists, IDatabaseView? view)> TryGetViewAsyncCore(IDatabaseViewProvider viewProvider, Identifier viewName, CancellationToken cancellationToken)
        {
            var viewOption = viewProvider.GetView(viewName, cancellationToken);
            var exists = await viewOption.IsSome.ConfigureAwait(false);
            var view = await viewOption.IfNoneUnsafe(default(IDatabaseView)!).ConfigureAwait(false);

            return (exists, view);
        }
    }
}
