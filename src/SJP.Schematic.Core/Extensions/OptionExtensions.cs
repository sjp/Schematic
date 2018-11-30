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
        public static T UnwrapSome<T>(this Option<T> input)
        {
            if (input.IsNone)
                throw new ArgumentException("The given optional object does not have a value.", nameof(input));

            return input.IfNoneUnsafe(default(T));
        }

        public static async Task<T> UnwrapSomeAsync<T>(this OptionAsync<T> input)
        {
            var isNone = await input.IsNone.ConfigureAwait(false);
            if (isNone)
                throw new ArgumentException("The given optional object does not have a value.", nameof(input));

            return await input.IfNoneUnsafe(default(T)).ConfigureAwait(false);
        }

        public static Option<T> FirstSome<T>(this IEnumerable<Option<T>> input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return input.FirstOrDefault(x => x.IsSome);
        }

        public static OptionAsync<T> FirstSomeAsync<T>(this IEnumerable<OptionAsync<T>> input, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return FirstSomeAsyncCore(input, cancellationToken).ToAsync();
        }

        private static async Task<Option<T>> FirstSomeAsyncCore<T>(IEnumerable<OptionAsync<T>> input, CancellationToken cancellationToken)
        {
            foreach (var option in input)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException(cancellationToken);

                var optionIsSome = await option.IsSome.ConfigureAwait(false);
                if (optionIsSome)
                    return await option.UnwrapSomeAsync().ConfigureAwait(false);
            }

            return Option<T>.None;
        }
    }
}
