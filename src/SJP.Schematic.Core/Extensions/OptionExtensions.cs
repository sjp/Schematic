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
    /// Returns the first <see cref="Option{T}"/> instance that is in the <see cref="Option{T}.IsSome"/> state.
    /// </summary>
    /// <param name="input">The source collection to reduce.</param>
    /// <returns>The <see cref="Option{T}"/> instance in the <paramref name="input"/> collection in the 'some' state, otherwise an <see cref="Option{T}"/> instance in the 'none' state when no 'some' options exist.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null" />.</exception>
    public static Option<T> FirstSome<T>(this IEnumerable<Option<T>> input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return input.FirstOrDefault(static x => x.IsSome);
    }

    /// <summary>
    /// Returns the first <see cref="OptionAsync{T}"/> instance that is in the <see cref="OptionAsync{T}.IsSome"/> state.
    /// </summary>
    /// <param name="input">The source collection to reduce.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <see cref="OptionAsync{T}"/> instance in the <paramref name="input"/> collection in the 'some' state, otherwise an <see cref="OptionAsync{T}"/> instance in the 'none' state when no 'some' options exist.</returns>
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

            var resolvedOption = await option.ToOption();
            if (resolvedOption.IsSome)
                return resolvedOption;
        }

        return Option<T>.None;
    }

    /// <summary>
    /// Returns <paramref name="first"/> if it resolves to a 'some' state; otherwise lazily invokes
    /// <paramref name="second"/> and returns its result.
    /// </summary>
    /// <remarks>
    /// Unlike the <c>|</c> operator on <see cref="OptionAsync{A}"/>, which evaluates both operands
    /// before combining them, this defers invoking <paramref name="second"/> until <paramref name="first"/>
    /// is known to be 'none' — useful when producing the second option is itself expensive (e.g. issues a
    /// database query), so that a match on <paramref name="first"/> avoids that work entirely.
    /// </remarks>
    /// <typeparam name="T">The type of value contained within the option.</typeparam>
    /// <param name="first">The option to try first.</param>
    /// <param name="second">A factory for the option to try when <paramref name="first"/> is 'none'.</param>
    /// <returns>An <see cref="OptionAsync{T}"/> in the 'some' state if either <paramref name="first"/> or the option produced by <paramref name="second"/> is 'some'; otherwise an option in the 'none' state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="second"/> is <see langword="null" />.</exception>
    public static OptionAsync<T> OrElse<T>(this OptionAsync<T> first, Func<OptionAsync<T>> second)
    {
        ArgumentNullException.ThrowIfNull(second);

        return OrElseAsyncCore(first, second).ToAsync();
    }

    private static async Task<Option<T>> OrElseAsyncCore<T>(OptionAsync<T> first, Func<OptionAsync<T>> second)
    {
        var firstResolved = await first.ToOption();
        if (firstResolved.IsSome)
            return firstResolved;

        return await second().ToOption();
    }
}