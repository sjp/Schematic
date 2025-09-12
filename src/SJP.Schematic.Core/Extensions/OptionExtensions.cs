using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// Extensions for working with <see cref="Option{A}"/> and <see cref="OptionAsync{A}"/> instances.
/// </summary>
public static class OptionExtensions
{
    /// <summary>
    /// Returns the first <see cref="Option{A}"/> instance that is in the <see cref="Option{A}.IsSome"/> state.
    /// </summary>
    /// <param name="input">The source collection to reduce.</param>
    /// <returns>The <see cref="Option{A}"/> instance in the <paramref name="input"/> collection in the 'some' state, otherwise an <see cref="Option{A}"/> instance in the 'none' state when no 'some' options exist.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null" />.</exception>
    public static Option<T> FirstSome<T>(this IEnumerable<Option<T>> input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return input.FirstOrDefault(static x => x.IsSome);
    }

    /// <summary>
    /// Returns the first <see cref="OptionAsync{A}"/> instance that is in the <see cref="OptionAsync{A}.IsSome"/> state.
    /// </summary>
    /// <param name="input">The source collection to reduce.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <see cref="OptionAsync{A}"/> instance in the <paramref name="input"/> collection in the 'some' state, otherwise an <see cref="OptionAsync{A}"/> instance in the 'none' state when no 'some' options exist.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null" />.</exception>
    public static OptionAsync<T> FirstSome<T>(this IEnumerable<OptionAsync<T>> input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

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