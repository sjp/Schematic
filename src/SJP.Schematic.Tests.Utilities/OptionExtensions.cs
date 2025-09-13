using System;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Tests.Utilities;

/// <summary>
/// Convenience methods for testing the values of <see cref="Option{A}"/> and <see cref="OptionAsync{A}"/> types.
/// </summary>
public static class OptionExtensions
{
    /// <summary>
    /// Unwraps an option type in the some state and returns its contained value.
    /// </summary>
    /// <typeparam name="T">The type of value contained in the <see cref="Option{T}"/> type.</typeparam>
    /// <param name="input">The input.</param>
    /// <returns>The contained value in <paramref name="input"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is an <see cref="Option{T}"/> in the none state.</exception>
    public static T UnwrapSome<T>(this Option<T> input)
    {
        if (input.IsNone)
            throw new ArgumentException("The given optional object does not have a value.", nameof(input));

        return (T)input;
    }

    /// <summary>
    /// Unwraps an option type in the some state and asynchronously returns its contained value.
    /// </summary>
    /// <typeparam name="T">The type of value contained in the <see cref="Option{T}"/> type.</typeparam>
    /// <param name="input">The input.</param>
    /// <returns>The contained value in <paramref name="input"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="input"/> is an <see cref="OptionAsync{T}"/> in the none state.</exception>
    public static async Task<T> UnwrapSomeAsync<T>(this OptionAsync<T> input)
    {
        var isNone = await input.IsNone.ConfigureAwait(false);
        if (isNone)
            throw new ArgumentException("The given optional object does not have a value.", nameof(input));

        return await input.IfNoneUnsafe(default(T)!).ConfigureAwait(false);
    }
}