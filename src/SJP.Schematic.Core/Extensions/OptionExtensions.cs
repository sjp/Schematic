using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Extensions
{
    public static class OptionExtensions
    {
        public static Option<T> FirstSome<T>(this IEnumerable<Option<T>> input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return input.FirstOrDefault(x => x.IsSome);
        }

        public static OptionAsync<T> FirstSome<T>(this IEnumerable<OptionAsync<T>> input, CancellationToken cancellationToken = default)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return FirstSomeAsyncCore(input, cancellationToken).ToAsync();
        }

        private static async Task<Option<T>> FirstSomeAsyncCore<T>(IEnumerable<OptionAsync<T>> input, CancellationToken cancellationToken)
        {
            foreach (var option in input)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var resolvedOption = await option.ToOption().ConfigureAwait(false);
                if (resolvedOption.IsSome)
                    return resolvedOption;
            }

            return Option<T>.None;
        }
    }
}
